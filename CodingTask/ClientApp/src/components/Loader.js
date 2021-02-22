import React, { PureComponent } from 'react';

export default class Loader extends PureComponent {
    render() {
        return (
            <div className="col-12 text-center">
                <div className="spinner-border text-info" role="status">
                    <span className="sr-only">Loading...</span>
                </div>
            </div>
        )
    }
}
