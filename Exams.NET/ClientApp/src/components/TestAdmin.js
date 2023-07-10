import React, {Component, useState} from 'react';
import authService from './api-authorization/AuthorizeService'
import {Button, Col, Form, FormGroup, Input, Label, Modal, ModalBody, ModalHeader} from "reactstrap";
import {Link} from "react-router-dom";

export class TestAdmin extends Component {
    static displayName= TestAdmin.name;

    constructor(props) {
        super(props);
        this.state = { Tests: [], loading: true };
    }

    componentDidMount() {
    this.populateTests().then(x=>console.log("fetched: "+x)).catch(x=>console.log("failed to fetch: " + x));
    }

    static TestAdminTable(tests, parentUpdate, deleteTest) {
        return (
            <div>
                <NewTestForm ParentCallback={parentUpdate}/>
                <table className="table table-striped" aria-labelledby="tableLabel">
                    <thead>
                    <tr>
                        <th>Test name</th>
                        <th>Created</th>
                        <th>Updated</th>
                        <th>Actions</th>
                    </tr>
                    </thead>
                    <tbody>
                    {tests.map(Test =>
                        <tr key={Test.testId}>
                            <td>{Test.testTitle}</td>
                            <td>{Test.created}</td>
                            <td>{Test.lastUpdated}</td>
                            <td><Button onClick={() => deleteTest(Test.testId)}>Delete</Button></td>
                        </tr>
                    )}
                    </tbody>
                </table>
            </div>
        );
    }

    async deleteTest( testId ) {
        console.log(testId);
        const token = await authService.getAccessToken();
        await fetch(`api/admin/Test/${testId}`, {
            method : "DELETE",
            headers: !token?{}:{'Authorize' : `Bearer ${token}`}
        })
            .then(res => res.ok&&this.populateTests())
            .catch(err => console.log(err));
    }

    render() {
        let contents = this.state.loading
            ? <p><em>Loading...</em></p>
            : TestAdmin.TestAdminTable(this.state.Tests, this.populateTests(), this.deleteTest);

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
        }).then(res => {
                if(!res.ok) {
                    console.log(res);
                    this.setState({loading : false});
                }
                else {
                    return res.json();
                }
            })
            .then(data => this.setState({Tests:data, loading: false}));
    }
}

function NewTestForm( { ParentCallback } ) {
    const [modal,setModal] = useState(false);
    const toggle = () => setModal(!modal);
    const [testForm, setTestForm] = useState({testTitle:""});
    const [submittable, setSubmittable] = useState(true);
        
    const handleSubmit = async (prop) => {
        prop.preventDefault();
        await submitTest(testForm);
        await ParentCallback();
    }
    
    const handleFormChanges = (event) => {
        event.preventDefault();
        const {name, value} = event.target;
        setTestForm({...testForm, [name]: value});
        
        if(name === "testTitle")
            setSubmittable(value==="");
    }
    
    const submitTest= async (prop) => {
        const token = await authService.getAccessToken();
        await fetch('api/admin/Test', {
            method : "POST",
            headers : !token? {} : {'Authorization' : `Bearer ${token}`, 'Content-Type' : 'application/json'},
            body : JSON.stringify({...prop})
        })
            .then(response => response.json())
            .then(data => console.log(data))
            .catch(err => console.log(err));
    }
    
    return (
        <div>
            <button className={"btn btn-primary"} onClick={toggle}>Create New Test</button>
            <Modal isOpen={modal} toggle={toggle}>
                <ModalHeader>Create New Test</ModalHeader>
                <ModalBody>
                    <Form onSubmit={handleSubmit}>
                        <FormGroup id={"test"} row>
                            <Col>
                                <Label htmlFor={"testTitle"} placeholder={"Test Name"} hidden={true}>TestTitle</Label>
                                <Input id={"testTitle"} 
                                       name={"testTitle"} 
                                       onChange={handleFormChanges} 
                                       placeholder={"Test Title"} 
                                       value={testForm.testTitle}/>
                            </Col>
                        </FormGroup>
                        <button className={"btn btn-primary text-center"} disabled={submittable}>
                            Submit
                        </button>
                    </Form>
                </ModalBody>
            </Modal>
        </div>
    )
}