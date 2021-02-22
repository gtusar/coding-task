import React, { Component } from 'react';
import { Route } from 'react-router';
import { Layout } from './components/Layout';
import { Home } from './components/Home';
import { FetchData } from './components/FetchData';
import { Counter } from './components/Counter';
import { OrderBook } from './components/OrderBook';
import { AuditLog } from './components/AuditLog';

import './custom.css'

export default class App extends Component {
  static displayName = App.name;

  render () {
    return (
      <Layout>
        <Route exact path='/' component={Home} />
        <Route path='/orderbook' component={OrderBook} />
        <Route path='/auditlog' component={AuditLog} />
      </Layout>
    );
  }
}
