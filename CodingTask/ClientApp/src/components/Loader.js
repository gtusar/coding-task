import React, { PureComponent } from 'react';

/**
 * Show rotating loading indicator
 * */
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
