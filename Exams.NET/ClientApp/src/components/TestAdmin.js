import React, { Component } from 'react';
import authService from './api-authorization/AuthorizeService'

export class TestAdmin extends Component {
    static displayName= TestAdmin.name;

    constructor(props) {
        super(props);
        this.state = { Tests: [], loading: true };
    }

    componentDidMount() {
    this.populateTests().then(x=>console.log("fetched: "+x)).catch(x=>console.log("failed to fetch: " + x));
    }

    static TestAdminTable(tests) {
        return (
            <table className="table table-striped" aria-labelledby="tableLabel">
                <thead>
                <tr>
                    <th>Created</th>
                    <th>Updated</th>
                    <th>Test name</th>
                    {/*<th>Total Question Count</th>*/}
                    {/*<th>Free Form Question Count</th>*/}
                    {/*<th>Multiple Choice Question Count</th>*/}
                    {/*<th>Total Point Value</th>*/}
                </tr>
                </thead>
                <tbody>
                {tests.map(Test =>
                    <tr key={Test}>
                        <td>{Test.created}</td>
                        <td>{Test.lastUpdated}</td>
                        <td>{Test.testTitle}</td>
                    </tr>
                )}
                </tbody>
            </table>
        );
    }

    render() {
        let contents = this.state.loading
            ? <p><em>Loading...</em></p>
            : TestAdmin.TestAdminTable(this.state.Tests);

        return (
            <div>
                <h1 id="tableLabel">Test Administration</h1>
                <p>This is for the high level administration of test.</p>
                {contents}
            </div>
        );
    }

    async populateTests() {
        const token = await authService.getAccessToken();
        const response = await fetch('api/admin/test', {
            headers: !token ? {} : { 'Authorization': `Bearer ${token}`, 'Accept': 'application/json' },
        });
        const data = await response.json();
        this.setState({ Tests: data, loading: false });
    }
}
