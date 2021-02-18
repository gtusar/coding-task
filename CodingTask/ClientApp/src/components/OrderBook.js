import React, { Component, useState } from 'react';
import { Toast, ToastBody, ToastHeader, Button } from 'reactstrap';
import { ResponsiveBar } from '@nivo/bar';
import * as signalR from "@microsoft/signalr";
import { extend } from 'jquery';

const OrderBookChart = ({ data }) => (
    <ResponsiveBar
        data={data}
        keys={['bids', 'asks']}
        indexBy="price"
        margin={{ top: 50, right: 130, bottom: 70, left: 60 }}
        padding={0.3}
        valueScale={{ type: 'linear' }}
        indexScale={{ type: 'band', round: true }}
        colors={{ scheme: 'set2' }}
        colorBy="id"
        axisBottom={{
            tickSize: 5,
            tickPadding: 5,
            tickRotation: -50,
            legend: 'Price',
            legendPosition: 'middle',
            legendOffset: 32
        }}
        axisLeft={{
            tickSize: 5,
            tickPadding: 5,
            tickRotation: 0,
            legend: 'Volume',
            legendPosition: 'middle',
            legendOffset: -40
        }}
        labelSkipWidth={12}
        labelSkipHeight={12}
        labelTextColor={{ from: 'color', modifiers: [['darker', 1.6]] }}
        legends={[
            {
                dataFrom: 'keys',
                anchor: 'bottom-right',
                direction: 'column',
                justify: false,
                translateX: 120,
                translateY: 0,
                itemsSpacing: 2,
                itemWidth: 100,
                itemHeight: 20,
                itemDirection: 'left-to-right',
                itemOpacity: 0.85,
                symbolSize: 20,
                effects: [
                    {
                        on: 'hover',
                        style: {
                            itemOpacity: 1
                        }
                    }
                ]
            }
        ]}
        padding={0}
        enableLabel={false}
        enableGridX={true}
        animate={false}
        motionstiffness={150}
        motiondamping={1}
    />
)

const connection = new signalR
    .HubConnectionBuilder()
    .withUrl("/orderbookhub")
    .withAutomaticReconnect().build();

export class OrderBook extends Component {
    static displayName = OrderBook.name;
    constructor(props) {
        super(props);
        this.state = {
            loading: true,
            tradingPair: "",
            availableTradingPairs: [],
            orderBookData: [],
            marketDepthChartData: [],
            orderBookChartData: [],
            allowChartUpdate: true
        }
        this.onDataReceived = this.onDataReceived.bind(this);
        this.updateCharts = this.updateCharts.bind(this);
    }

    async componentDidMount() {
        await this.populateTradingPairsData();

        connection.start()
            .then(() => {
                console.info('SignalR Connected');
                this.startReceivingData();
            })
            .catch(err => console.error('SignalR Connection Error: ', err));
        connection.on('connected', this.onSocketConnected);
        connection.on('dataReceived', this.onDataReceived);
        connection.on('disconnected', this.onNotifReceived);
    }
    async componentWillUnmount() {
        await connection.invoke("StopReceivingData");
        await connection.stop();
    }
    startReceivingData() {
        connection.invoke("StartReceivingData", this.state.selectedTradingPair);
    }
    onSocketConnected(res) {
        console.info('SocketConnected', res);
        this.startReceivingData();
    }
    onNotifReceived(res) {
        console.info('Yayyyyy, I just received a notification!!!', res);
    }
    onDataReceived(res) {
        // update order book table
        this.setState({ orderBookData: res.orderBookData });
        // update charts if allowed
        if (this.state.allowChartUpdate) {
           this.updateCharts(res);
        }
    }

    updateCharts(response) {
        // update state with chart data
        this.setState({
            orderBookChartData: response.orderBookChartData,
            marketDepthChartData: response.marketDepthChartData,
            allowChartUpdate: false
        });
        // set timer to disable too frequent chart update
        setTimeout(() => { this.setState({ allowChartUpdate: true }) }, 2000);
    }
    async selectTradingPair(event) {
        this.setState({ selectedTradingPair: event.target.value });
        await connection.invoke("StopReceivingData")
            .then(async () => await connection.invoke("StartReceivingData", this.state.selectedTradingPair));
        
    }
    render() {
        return (
            <div>
                <div className="row justify-content-center">
                    <div className="col-sm-6 text-center">
                        <h1 className="display-6">Order Book</h1>
                    </div>
                </div>
                <div className="row">
                    <div className="col-sm-3">
                        <select className="form-control" onChange={(e) => this.selectTradingPair(e) }>
                            {this.state.availableTradingPairs.map(pair => <option key={pair.apiName} value={pair.apiName}>{pair.title}</option>)}
                        </select>
                    </div>
                </div>
                <div className="row">
                    <div className="col-7" style={{ "width": "0.6vw", "height": "350px" }}>
                        
                    </div>
                    <div className="col-5">
                        <OrderBookTable data={this.state.orderBookData} />
                    </div>
                </div>
            </div>
        );
    }
    //{OrderBookChart({ data: this.state.orderBookChartData })}
    //{ OrderBookChart({ data: this.state.marketDepthChartData }) }
    
    async populateTradingPairsData() {
        const response = await fetch('tradingpairs');
        const data = await response.json();
        this.setState({
            availableTradingPairs: data || [],
            selectedTradingPair: data[0].apiName, // select first trading pair as default
            loading: false
        });
    }
}

class OrderBookTable extends Component {

    renderRows() {
        const data = this.props.data;
        const bids = data.filter(d => d.bids > 0) || [];
        const asks = data.filter(d => d.asks > 0) || [];
        let rows = [];
        for (let i = 0; i < Math.max(bids.length, asks.length); i++) {
            const bid = bids[i];
            const ask = asks[i];
            const bidPrice = Number.parseFloat(bid.price);
            const askPrice = Number.parseFloat(ask.price);
            rows.push(
                <tr key={i}>
                    <td>{(bidPrice * bid.bids).toFixed(4)}</td>
                    <td>{bid.bids.toFixed(4)}</td>
                    <td className="text-success">{bidPrice.toFixed(2)}</td>
                    <td className="text-danger">{askPrice.toFixed(2)}</td>
                    <td>{ask.asks.toFixed(4)}</td>
                    <td>{(askPrice * ask.asks).toFixed(4)}</td>
                </tr>
            );
        }
        return rows;
    }

    render() {
        return (
            <div className="container">
                <div className="row">
                    <div className="col">
                        <table className="table table-striped table-responsive table-condensed small">
                            <thead>
                                <tr className="strong">
                                    <th>Value</th>
                                    <th>Amount</th>
                                    <th className="text-success">Bid</th>
                                    <th className="text-danger">Ask</th>
                                    <th>Amount</th>
                                    <th>Value</th>
                                </tr>
                            </thead>
                            <tbody>
                                {this.renderRows()}
                            </tbody>
                        </table>
                    </div>
                </div>
            </div>
        );
    }
}