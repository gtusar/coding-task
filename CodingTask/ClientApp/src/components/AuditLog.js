import React, { Component, PureComponent } from 'react';
import { BarChart, Bar, Cell, XAxis, YAxis, CartesianGrid, Tooltip, Legend, ResponsiveContainer } from 'recharts';

const data = [{ "timestamp": "2021-02-21T22:38:00Z", "price": 47088.904 }, { "timestamp": "2021-02-21T22:38:01Z", "price": 47088.90 }, { "timestamp": "2021-02-21T22:38:02Z", "price": 47082.612 }, { "timestamp": "2021-02-21T22:38:03Z", "price": 47057.49 }, { "timestamp": "2021-02-21T22:38:04Z", "price": 47057.49 }, { "timestamp": "2021-02-21T22:38:05Z", "price": 47057.49 }, { "timestamp": "2021-02-21T22:38:06Z", "price": 47057.49 }, { "timestamp": "2021-02-21T22:38:07Z", "price": 47087.334 }, { "timestamp": "2021-02-21T22:38:08Z", "price": 47088.91 }, { "timestamp": "2021-02-21T22:38:09Z", "price": 47088.91 }, { "timestamp": "2021-02-21T22:38:10Z", "price": 47090.37 }, { "timestamp": "2021-02-21T22:38:11Z", "price": 47090.37 }, { "timestamp": "2021-02-21T22:38:12Z", "price": 47090.37 }, { "timestamp": "2021-02-21T22:38:13Z", "price": 47090.37 }, { "timestamp": "2021-02-21T22:38:14Z", "price": 47090.37 }, { "timestamp": "2021-02-21T22:38:15Z", "price": 47091.42 }, { "timestamp": "2021-02-21T22:38:16Z", "price": 47092.12 }, { "timestamp": "2021-02-21T22:38:17Z", "price": 47092.12 }, { "timestamp": "2021-02-21T22:38:18Z", "price": 47092.12 }, { "timestamp": "2021-02-21T22:38:19Z", "price": 47092.12 }, { "timestamp": "2021-02-21T22:38:20Z", "price": 47092.12 }, { "timestamp": "2021-02-21T22:38:21Z", "price": 47092.12 }, { "timestamp": "2021-02-21T22:38:22Z", "price": 47092.12 }, { "timestamp": "2021-02-21T22:38:23Z", "price": 47092.12 }, { "timestamp": "2021-02-21T22:38:24Z", "price": 47092.12 }, { "timestamp": "2021-02-21T22:38:25Z", "price": 47092.12 }, { "timestamp": "2021-02-21T22:38:26Z", "price": 47108.088 }, { "timestamp": "2021-02-21T22:38:27Z", "price": 47114.10 }, { "timestamp": "2021-02-21T22:38:28Z", "price": 47114.10 }, { "timestamp": "2021-02-21T22:38:29Z", "price": 47114.10 }, { "timestamp": "2021-02-21T22:38:30Z", "price": 47125.416 }, { "timestamp": "2021-02-21T22:38:31Z", "price": 47127.172 }, { "timestamp": "2021-02-21T22:38:32Z", "price": 47127.18 }, { "timestamp": "2021-02-21T22:38:33Z", "price": 47127.18 }, { "timestamp": "2021-02-21T22:38:34Z", "price": 47127.173333333333333333333333 }, { "timestamp": "2021-02-21T22:38:35Z", "price": 47127.17 }, { "timestamp": "2021-02-21T22:38:36Z", "price": 47127.1775 }, { "timestamp": "2021-02-21T22:38:37Z", "price": 47127.18 }, { "timestamp": "2021-02-21T22:38:38Z", "price": 47129.808 }, { "timestamp": "2021-02-21T22:38:39Z", "price": 47133.75 }, { "timestamp": "2021-02-21T22:38:40Z", "price": 47139.58 }, { "timestamp": "2021-02-21T22:38:41Z", "price": 47156.142 }, { "timestamp": "2021-02-21T22:38:42Z", "price": 47156.214 }, { "timestamp": "2021-02-21T22:38:43Z", "price": 47147.02 }, { "timestamp": "2021-02-21T22:38:44Z", "price": 47147.02 }, { "timestamp": "2021-02-21T22:38:45Z", "price": 47147.02 }, { "timestamp": "2021-02-21T22:38:46Z", "price": 47147.02 }, { "timestamp": "2021-02-21T22:38:47Z", "price": 47147.02 }, { "timestamp": "2021-02-21T22:38:48Z", "price": 47147.02 }, { "timestamp": "2021-02-21T22:38:49Z", "price": 47147.02 }, { "timestamp": "2021-02-21T22:38:50Z", "price": 47148.868 }, { "timestamp": "2021-02-21T22:38:51Z", "price": 47155.26 }, { "timestamp": "2021-02-21T22:38:52Z", "price": 47155.26 }, { "timestamp": "2021-02-21T22:38:53Z", "price": 47155.26 }, { "timestamp": "2021-02-21T22:38:54Z", "price": 47155.26 }, { "timestamp": "2021-02-21T22:38:55Z", "price": 47155.258 }, { "timestamp": "2021-02-21T22:38:56Z", "price": 47155.26 }, { "timestamp": "2021-02-21T22:38:57Z", "price": 47155.26 }, { "timestamp": "2021-02-21T22:38:58Z", "price": 47155.26 }, { "timestamp": "2021-02-21T22:38:59Z", "price": 47155.26 }, { "timestamp": "2021-02-21T22:39:00Z", "price": 47155.254 }, { "timestamp": "2021-02-21T22:39:01Z", "price": 47149.748 }, { "timestamp": "2021-02-21T22:39:02Z", "price": 47139.078 }, { "timestamp": "2021-02-21T22:39:03Z", "price": 47132.884 }, { "timestamp": "2021-02-21T22:39:04Z", "price": 47120.42 }, { "timestamp": "2021-02-21T22:39:05Z", "price": 47118.26 }, { "timestamp": "2021-02-21T22:39:06Z", "price": 47109.67 }, { "timestamp": "2021-02-21T22:39:07Z", "price": 47101.06 }, { "timestamp": "2021-02-21T22:39:08Z", "price": 47101.06 }, { "timestamp": "2021-02-21T22:39:09Z", "price": 47101.06 }, { "timestamp": "2021-02-21T22:39:10Z", "price": 47101.06 }, { "timestamp": "2021-02-21T22:39:11Z", "price": 47101.06 }, { "timestamp": "2021-02-21T22:39:12Z", "price": 47101.06 }];

export class AuditLog extends Component {
    static displayName = AuditLog.name;

    constructor(props) {
        super(props);
        this.state = { availableTimestamps: [], loading: true };
    }

    componentDidMount() {
        this.getAvailableTimestamps();
    }
    clickedTimestamp(timestamp) {
        alert(timestamp);
    }
    render() {
        let contents = this.state.loading
            ? <p><em>Loading...</em></p>
            : <AuditLogChart data={this.state.availableTimestamps} clickedTimestamp={this.clickedTimestamp} />;

        return (
            <div>
                <h1 id="tabelLabel" >Available timestamps</h1>
                {contents}
                <table className="table">
                    <tbody>
                        {this.state.availableTimestamps.map(t => <tr key={t.timestamp}>
                            <td>{t.timestamp}</td>
                            <td>{t.price}</td>
                        </tr>)}
                    </tbody>
                </table>

            </div>
        );
    }

    async getAvailableTimestamps() {
        const response = await fetch('AuditLogData');
        const data = await response.json();
        this.setState({ availableTimestamps: data, loading: false });
    }
}


export default class AuditLogChart extends PureComponent {
    static demoUrl = 'https://codesandbox.io/s/bar-chart-with-customized-event-4k1bd';

    state = {
        data: this.props.data,
        activeIndex: 0,
    };

    handleClick = (data, index) => {
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
                
                <ResponsiveContainer width="100%" height={250}>
                    <BarChart width={150} height={150} data={data}>
                        <Tooltip />
                        <CartesianGrid strokeDasharray="3 3" />
                        <XAxis dataKey="timestamp" />
                        <YAxis domain={['auto', 'auto']} />
                        <Bar dataKey="price" onClick={this.handleClick}>
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
