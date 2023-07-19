import React, {useEffect, useState} from 'react';
import authService from './api-authorization/AuthorizeService'
import {Button, CloseButton} from "reactstrap";
import TestForm from "./TestAdminForm";
import { FaEdit } from 'react-icons/fa';

export default function TestAdmin() {
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
        return (
        <div>
            <table className="table table-striped" aria-labelledby="tableLabel">
                <thead>
                <tr>
                    <th>Test name</th>
                    <th>Description</th>
                    <th>Created</th>
                    <th>Updated</th>
                    <th className="d-flex align-items-center justify-content-end" style={{paddingRight: 12}}>Actions</th>
                </tr>
                </thead>
                <tbody>
                {
                    Array.isArray(tests) && tests.map(test =>
                    <tr key={test.testId} className={"align-middle"}>
                            <td>{test.testTitle}</td>
                            <td>{test.testDescription}</td>
                            <td>{test.created}</td>
                            <td>{test.lastUpdated}</td>
                            <td className="d-flex align-items-center justify-content-end">
                                <Button color="link" style={{paddingTop: '0', marginRight: 0}} onClick={() => editTest(test)}>
                                    <FaEdit style={{fontSize: 22}} />
                                </Button>
                                <CloseButton close onClick={() => deleteTest(test.testId)}/>
                            </td>
                        </tr>
                    )
                }
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