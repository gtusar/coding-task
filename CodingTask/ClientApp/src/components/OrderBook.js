import React, { Component } from 'react';

export class OrderBook extends Component {
    static displayName = OrderBook.name;

    constructor(props) {
        super(props);
        this.state = {
            loading: true,
            availableTradingPairs: []
        }
    }

    componentDidMount() {
        this.populatePairsData();
    }
    render() {
        return (
            <div className="row">
                <div className="col-2">
                    <select className="form-control">
                        {this.state.availableTradingPairs.map(pair => <option value={pair.apiName}>{pair.title}</option>)}
                    </select>
                </div>
                <div className="col-6" style={{"background-color": "red"}}>
                    <div>Chart</div>
                </div>
            </div>
        );
    }

    async populatePairsData() {
        const response = await fetch('tradingpairs');
        const data = await response.json();
        this.setState({ availableTradingPairs: data, loading: false });
    }
}
