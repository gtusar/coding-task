import { BarChart, Bar, Cell, XAxis, YAxis, CartesianGrid, Tooltip, Legend, ResponsiveContainer } from 'recharts';
import React, { Component, PureComponent } from 'react';
import { OrderBookTable, OrderBookBarChart } from './OrderBook';
import Loader from './Loader';


export class AuditLog extends Component {

    constructor(props) {
        super(props);
        this.state = {
            availableTimestamps: [],
            loading: true,
            detailsLoading: false,
            orderBookDataAvailable: false,
            orderBookData: []
        };
        this.clickedTimestamp = this.clickedTimestamp.bind(this);
    }

    componentDidMount() {
        this.getAvailableTimestamps();
    }
    clickedTimestamp(timestamp) {
        this.setState({ detailsLoading: true });
        this.getOrderBookDetails(timestamp);
    }
   
    render() {
        const loadingOrChart = function (doShow, chart) {
            if (doShow) {
                return chart;
            }
            else {
                return <Loader />
            }
        }

        return (
            <div>
                <div className="row justify-content-center">
                    <div className="col-sm-6 text-center">
                        <h1 className="display-6">Audit log</h1>
                    </div>
                </div>
                <div className="row">
                    <div className="col-12 justify-content-center">
                        {loadingOrChart(!this.state.loading, <AuditLogChart data={this.state.availableTimestamps} clickedTimestamp={this.clickedTimestamp} />)}
                    </div>
                </div>
                {(this.state.detailsLoading || this.state.orderBookDataAvailable) &&
                    <div className="row">
                        <div className="col-sm-6">
                        {loadingOrChart(this.state.orderBookDataAvailable, <OrderBookTable data={this.state.orderBookData.orderBookData} clickedTimestamp={this.clickedTimestamp} />)}
                        </div>
                        <div className="col-sm-6">
                        {loadingOrChart(this.state.orderBookDataAvailable, <OrderBookBarChart data={this.state.orderBookData.marketDepthChartData} clickedTimestamp={this.clickedTimestamp} />)}
                        </div>
                    </div>
                }
            </div>
        );
    }

    async getAvailableTimestamps() {
        const response = await fetch('AuditLogData');
        const data = await response.json();
        this.setState({ availableTimestamps: data, loading: false });
    }

    async getOrderBookDetails(selectedTimestamp) {
        const requestOptions = {
            method: 'POST',
            dataType: 'json',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({ timestamp: selectedTimestamp })
        };

        const response = await fetch('AuditLogData', requestOptions);
        let data, orderBookDataAvailable;

        if (response.ok) { 
            data = await response.json();
            orderBookDataAvailable = true;
        }
        else
        {
            orderBookDataAvailable = false;
        }
        this.setState({ orderBookData: data, orderBookDataAvailable: orderBookDataAvailable, detailsLoading: false });
    }
}


export default class AuditLogChart extends PureComponent {
    state = {
        data: this.props.data,
        activeIndex: 0,
    };

    handleBarClicked = (data, index) => {
        this.props.clickedTimestamp(this.state.data[index].timestamp);
        this.setState({
            activeIndex: index,
        });
    };

    render() {
        const { activeIndex, data } = this.state;
        const activeItem = data[activeIndex];

        return (
            <div style={{ width: '100%' }}>
                <ResponsiveContainer width="100%" height={200}>
                    <BarChart width={150} height={150} data={data}>
                        <Tooltip />
                        <CartesianGrid strokeDasharray="3 3" />
                        <XAxis dataKey="timestamp" />
                        <YAxis domain={['auto', 'auto']} />
                        <Bar dataKey="price" onClick={this.handleBarClicked}>
                            {data.map((entry, index) => (
                                <Cell cursor="pointer" fill={index === activeIndex ? '#dc3545' : '#28a745'} key={`cell-${index}`} />
                            ))}
                        </Bar>
                    </BarChart>
                </ResponsiveContainer>
            </div>
        );
    }
}
