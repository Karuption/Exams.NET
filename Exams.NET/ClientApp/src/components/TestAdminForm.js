import {useEffect, useState} from "react";
import {Form, FormGroup, Input, Label, Modal, ModalBody, ModalHeader, Row} from "reactstrap";
import authService from "./api-authorization/AuthorizeService";

export default function TestForm( { ParentCallback, isOpen, toggle, headerText, editTest } ) {
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
    )
}