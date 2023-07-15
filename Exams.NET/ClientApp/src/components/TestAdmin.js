import React, {useEffect, useState} from 'react';
import authService from './api-authorization/AuthorizeService'
import {Button, Col, Form, FormGroup, Input, Label, Modal, ModalBody, ModalHeader, Row} from "reactstrap";

export const TestAdmin = () => {
    const [tests, setTests] = useState([])
    const [loading, setLoading] = useState(true);
    const [selectedTest, setSelectedTest] = useState({});
    const [testModal, setTestModal] = useState(false);
    
    useEffect(()=> {populateTests()}, []);
    
    let table = loading
        ? <p><em>Loading...</em></p>
        : TestAdminTable(tests, (test)=>{
            setSelectedTest(test);
            setTestModal(!testModal);
        }, (id)=>deleteTest(id));

    return (
        <div>
            <h1 id="tableLabel">Test Administration</h1>
            <p>This is for the high level administration of test.</p>
            <button className={"btn btn-primary"} onClick={()=>{setSelectedTest({});setTestModal(!testModal);}} >Create New Test</button>
            <TestForm ParentCallback={populateTests} 
                      toggle={()=>setTestModal(!testModal)}
                      isOpen={testModal}
                      headerText={"Create New Test"} 
                      editTest={selectedTest}/>
            {table}
        </div>
    );

    function TestAdminTable(tests, editTest, deleteTest) {
        return (<div>
            <table className="table table-striped" aria-labelledby="tableLabel">
                <thead>
                <tr>
                    <th>Test name</th>
                    <th>Description</th>
                    <th>Created</th>
                    <th>Updated</th>
                    <th>Actions</th>
                </tr>
                </thead>
                <tbody>
                {tests.map(test =>
                    <tr key={test.testId}>
                        <td>{test.testTitle}</td>
                        <td>{test.testDescription}</td>
                        <td>{test.created}</td>
                        <td>{test.lastUpdated}</td>
                        <td><Button onClick={() => {editTest(test);}}>Edit</Button></td>
                        <td><Button onClick={() => deleteTest(test.testId)}>Delete</Button></td>
                    </tr>
                )}
                </tbody>
            </table>
        </div>);
    }

    async function populateTests() {
        const token = await authService.getAccessToken();
        await fetch('api/admin/test', {
            headers: !token ? {} : { 'Authorization': `Bearer ${token}`, 'Accept': 'application/json' },
        }).then(res => {
            if(!res.ok) {
                console.log(res);
                setLoading( false);
            }
            else
                return res.json();
        })
            .then(data => {setTests(data); setLoading(false);});
    }


    async function deleteTest( testId ) {
        const token = await authService.getAccessToken();
        await fetch(`api/admin/Test/${testId}`, {
            method : "DELETE",
            headers: !token?{}:{'Authorization' : `Bearer ${token}`}
        })
            .then(res => res.ok&&populateTests())
            .catch(err => console.log(err));
    }
}

function TestForm( { ParentCallback, isOpen, toggle, headerText, editTest } ) {
    const [testForm, setTestForm] = useState(editTest);
    const [submitBlock, setSubmitBlock] = useState(true);
    const [editing,setEditing] = useState(Object.keys(editTest).length === 0);
    
    useEffect(()=> {
        if (editTest===null || Object.keys(editTest).length === 0) {
            setTestForm({testTitle: "", testDescription: ""});
            setEditing(false);
        }else {
            setTestForm(editTest);
            setEditing(true);
        }}, [editTest]);

    useEffect(()=>{
        const notChanged = Object.values(testForm).every((value, index) => value === Object.values(editTest)[index]);
        if(notChanged    || testForm.testTitle === ""){
            setSubmitBlock(true);
        } else
            setSubmitBlock(false);
    },[testForm,editTest])

    const handleSubmit = async (event) => {
        event.preventDefault();
        if(!editing)
            await submitNewTest(testForm);
        else 
            await submitEditedTest(testForm);
        toggle();
        await ParentCallback();
    }
    
    const handleFormChanges = (event) => {
        event.preventDefault();
        const {name, value} = event.target;
        setTestForm({...testForm, [name]: value});
    }
    
    const submitNewTest= async (prop) => {
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
    
    const submitEditedTest= async (prop) => {
        const token = await authService.getAccessToken();
        await fetch(`api/admin/Test/${prop.testId}`, {
            method : "PUT",
            headers : !token? {} : {'Authorization' : `Bearer ${token}`, 'Content-Type' : 'application/json'},
            body : JSON.stringify({...prop})
        })
            .then(response => {
                if(response.ok)
                    return response.json()
                else 
                    throw Error (`Unable to send response. ${JSON.stringify(response)}`);
            }).then(data => console.log(data))
            .catch(err => console.log(err));
    }
    
    return (
        <div>
            <Modal isOpen={isOpen} toggle={toggle}>
                <ModalHeader>{headerText}</ModalHeader>
                <ModalBody>
                    <Form onSubmit={handleSubmit}>
                        <FormGroup id={"test"} row>
                            <Row>
                                <Label htmlFor={"testTitle"} placeholder={"Test Name"} hidden={true}>TestTitle</Label>
                                <Input id={"testTitle"} 
                                       name={"testTitle"} 
                                       onChange={handleFormChanges} 
                                       placeholder={"Test Title"} 
                                       value={testForm.testTitle}/>
                            </Row>
                            <Row>
                                <Label htmlFor={"testDescription"} placeholder={"Description"} hidden={true}>testDescription</Label>
                                <Input id={"testDescription"}
                                       name={"testDescription"}
                                       onChange={handleFormChanges}
                                       placeholder={"Description"}
                                       type={"textarea"}
                                       value={testForm.testDescription} />
                            </Row>
                        </FormGroup>
                        <button className={"btn btn-primary text-center"} disabled={submitBlock}>
                            Submit
                        </button>
                    </Form>
                </ModalBody>
            </Modal>
        </div>
    )
}