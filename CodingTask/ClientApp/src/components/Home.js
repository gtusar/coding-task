import React, { Component } from 'react';
import welcomeImage from '../images/pexels-alesia-kozik-6772024.jpg';

/**
 * Home screen with task description and welcome image
 * */
export class Home extends Component {
    render() {
        return (
            <div className="row justify-content-center">
                <div className="col-xs-12 col-md-6">
                    <h1 className="display-3">The task!</h1>
                    <p>
                        Display the BTC / EUR market depth chart to the end user on a simple website, you can get the data from Bitstamp or any other exchange.
                        The back - end should be written in .NET, for the front - end you can choose any framework you wish or even do it without a framework.
                        To make our task a bit more challenging, you should keep an audit log of every order book that is[potentially] displayed to the end user with the timestamp of when it was acquired.
                    </p>
                    <ul>
                        <li>Write some tests for your code.</li>
                        <li>Show also the BTC / USD order book to the user.</li>
                    </ul>
                    <p>OTHER NOTES:</p>
                    <ul>
                        <li>Please use <code>GIT</code> while developing the code.</li>
                        <li>Keep it as simple as the nature of this problem allows it.</li>
                        <li>Do not over - engineer.</li>
                        <li>Be ready to explain / demonstrate your solution in our office(or in a remote chat).</li>
                    </ul>
                    <h2>May the force be with you.</h2>
                </div>
                <div className="col-sm-12 col-md-4 col-lg-3">
                    <img className="img-fluid img-thumbnail mx-auto" src={welcomeImage} ></img>
                </div>
            </div>
        );
    }
}
