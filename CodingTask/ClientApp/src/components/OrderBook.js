import React, { Component, PureComponent } from 'react';
import * as signalR from "@microsoft/signalr";
import { BarChart, Bar, XAxis, YAxis, CartesianGrid, Tooltip, Legend, ResponsiveContainer } from 'recharts';

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
        console.info('SocketConnected');
        this.startReceivingData();
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
        // const arrayPrototype = ['price','bids','asks'];
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
                    <div className="col-7" style={{ "width": "0.6vw", "height": "350px" }}>
                        <OrderBookBarChart data={this.state.orderBookChartData} />
                        <OrderBookBarChart data={this.state.marketDepthChartData} />
                    </div>
                    <div className="col-5">
                        <div className="col-md-5">
                            <select className="form-control" onChange={(e) => this.selectTradingPair(e)}>
                                {this.state.availableTradingPairs.map(pair => <option key={pair.apiName} value={pair.apiName}>{pair.title}</option>)}
                            </select>
                        </div>
                        <OrderBookTable data={this.state.orderBookData} />
                    </div>
                </div>
            </div>
        );
    }
    
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

export class OrderBookTable extends Component {

    renderRows() {
        const data = this.props.data;
        const bids = data.filter(d => d.bids > 0) || [];
        const asks = data.filter(d => d.asks > 0) || [];
        let rows = [];
        const maxRows = Math.max(bids.length, asks.length);
        for (let i = 0; i < maxRows; i++) {
            const bid = bids[bids.length - 1 - i] || {price:'',asks:0, bids:0}; // reverse order
            const ask = asks[i] || { price: '', asks: 0, bids: 0 };
            const bidPrice = Number.parseFloat(bid.price.replace(',', '.'));
            const askPrice = Number.parseFloat(ask.price.replace(',', '.'));
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

export class OrderBookBarChart extends PureComponent {
    render() {
        return (
            <ResponsiveContainer width="100%" height="100%">
                <BarChart
                    width={400}
                    height={250}
                    data={this.props.data}
                    margin={{
                        top: 20,
                        right: 30,
                        left: 20,
                        bottom: 5,
                    }}
                >
                    <CartesianGrid strokeDasharray="3 3" />
                    <XAxis dataKey="price" />
                    <YAxis />
                    <Tooltip />
                    <Legend />
                    <Bar dataKey="bids" stackId="a" fill="#28a745" />
                    <Bar dataKey="asks" stackId="a" fill="#dc3545" />
                </BarChart>
            </ResponsiveContainer>
        );
    }
}
