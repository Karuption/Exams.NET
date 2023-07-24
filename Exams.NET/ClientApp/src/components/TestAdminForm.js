import {useEffect, useState} from "react";
import {Form, FormFeedback, FormGroup, Input, Label, Modal, ModalBody, ModalHeader, Row} from "reactstrap";
import authService from "./api-authorization/AuthorizeService";
import {FaL} from "react-icons/fa6";

export default function TestForm( { ParentCallback, isOpen , toggle , headerText = "Create a new test", editTest } ) {
    const [testForm, setTestForm] = useState(editTest);
    const [testFormValidation, setTestFormValidation] = useState({isValidTitle: true, isValidDescription: true, titleError:"", descriptionError:""});
    const [submitBlock, setSubmitBlock] = useState(true);
    const [editing,setEditing] = useState(Object.keys(editTest).length === 0);

    // Test form editing, if its null/undefined/empty, then we are editing
    useEffect(()=> {
        if (editTest===null || Object.keys(editTest).length === 0) {
            setTestForm({testTitle: "", testDescription: ""});
            setEditing(false);
        }else {
            setTestForm(editTest);
            setEditing(true);
        }}, [editTest]);

    // set submit block and validate the test
    useEffect(()=>{
        if((editing && testForm === editTest) || testForm.testTitle === ""){
            setTestFormValidation({...testFormValidation, isValidDescription: false, titleError: "This cannot be empty"});
            return setSubmitBlock(true);
        }
        setSubmitBlock(false);
    },[testForm,editTest, editing])
    
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
                        <FormGroup id={"test"} floating={true}>
                            <Input id={"testTitle"}
                                   name={"testTitle"}
                                   invalid={!testFormValidation.isValidTitle}
                                   onChange={handleFormChanges}
                                   placeholder={"Test Title"}
                                   value={testForm.testTitle}/>
                            <Label for={"testTitle"} >Test Title</Label>
                            <FormFeedback valid={false}>{testFormValidation.descriptionError}</FormFeedback>
                        </FormGroup>
                        <FormGroup floating={true}>
                            <Input id={"testDescription"}
                                   name={"testDescription"}
                                   onChange={handleFormChanges}
                                   type={"textarea"}
                                   value={testForm.testDescription} />
                            <Label for={"testDescription"}>testDescription</Label>
                        </FormGroup>
                        <button className={"btn btn-primary text-center"} disabled={submitBlock}>
                            Submit
                        </button>
                    </Form>
                </ModalBody>
        </Modal>
    )
}