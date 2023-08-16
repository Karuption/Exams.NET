import React, {useEffect, useState} from "react";
import {
    Breadcrumb, BreadcrumbItem,
    Button, Card, CardBody, CardHeader, CardSubtitle, CloseButton,
    Dropdown,
    DropdownItem,
    DropdownMenu,
    DropdownToggle,
    Form,
    FormFeedback,
    FormGroup,
    Input,
    Label,
} from "reactstrap";
import authService from "./api-authorization/AuthorizeService";
import {QuestionPopUp, QuestionTextView} from "./QuestionControls";

export default function TestForm( { ParentCallback , editTest } ) {
    const [testForm, setTestForm] = useState(editTest);
    const [testFormValidation, setTestFormValidation] = useState({isValidTitle: true, isValidDescription: true, titleError:"", descriptionError:""});
    const [submitBlock, setSubmitBlock] = useState(true);
    const [editing,setEditing] = useState(Object.keys(editTest).length === 0);
    const [problems, setProblems] = useState([]);
    const [questionPopUpOpen, setQuestionPopUpOpen] = useState(false);

    // Test form editing, if its null/undefined/empty, then we are editing
    useEffect(()=> {
        if (editTest===null || Object.keys(editTest).length === 0) {
            setTestForm({testTitle: "", testDescription: ""});
            setEditing(false);
        }else {
            setTestForm(editTest);
            setEditing(true);
        }
        getAllQuestions();
        setProblems(editTest.problems);
        }, [editTest]);

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
        if(problems && problems !== [])
            testForm.problems = [...problems];
        if(!editing)
            await submitNewTest(testForm);
        else
            await submitEditedTest(testForm);
        await ParentCallback();
    }

    const handleFormChanges = (event) => {
        event.preventDefault();
        const {name, value} = event.target;
        setTestForm({...testForm, [name]: value});
    }

    const submitNewTest = async (prop) => {
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

    const submitEditedTest = async (prop) => {
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
            <Form onSubmit={handleSubmit}>
                <FormGroup id={"test"} floating={true}>
                    <Input id={"testTitle"}
                           name={"testTitle"}
                           invalid={!testFormValidation.isValidTitle}
                           onChange={handleFormChanges}
                           placeholder={"Test Title"}
                           value={testForm.testTitle} />
                    <Label for={"testTitle"}>Test Title</Label>
                    <FormFeedback valid={false}>{testFormValidation.descriptionError}</FormFeedback>
                </FormGroup>
                <FormGroup floating={true}>
                    <Input id={"testDescription"}
                           name={"testDescription"}
                           onChange={handleFormChanges}tttt--
                           type={"textarea"}
                           value={testForm.testDescription} />
                    <Label for={"testDescription"}>Test Description</Label>
                </FormGroup>
                <div className={'d-flex justify-content-center my-2'}>
                    <h5 className={'my-0'}>Test Questions</h5>
                </div>
                {
                    problems.map(problem=> (
                        <Card className={'my-3'}>
                            <CloseButton className={'position-absolute top-0 end-0 me-2 mt-2'} />
                            <CardHeader>{problem.choice?'Multiple choice':'Free Answer'}</CardHeader>
                            <CardBody>
                                <QuestionTextView question={problem} />
                            </CardBody>
                        </Card>
                    ))
                }
                <FormGroup>
                    <QuestionDropdownMenu questions={problems}/>
                </FormGroup>
                <Button className={"btn btn-primary text-center"} disabled={submitBlock}>
                    Submit
                </Button>
            </Form>
    )
    async function getAllQuestions() {
        const token = await authService.getAccessToken();
        await fetch('api/admin/Question', {
            headers: !token ? {} : { 'Authorization': `Bearer ${token}`, 'Accept': 'application/json' },
        }).then(res => {
            if(!res.ok)
                console.log(res);
            else
                return res.json();
        })
            .then(data => setProblems(data));
    }
}

function QuestionDropdownMenu({questions = [], onQuestionSelection = e => {}}) {
    const [open, setOpen] = useState(false);
    const [questionPopupOpen, setQuestionPopupOpen] = useState(false);

    return (
        <>
            <Dropdown isOpen={open} toggle={()=>setOpen(!open)} direction={'down'}>
                <DropdownToggle caret={true} color={'primary'}>Add a test question</DropdownToggle>
                <DropdownMenu end={true}>
                    <DropdownItem disabled={questions.length<1} onClick={()=>setQuestionPopupOpen(true)}>Clone existing</DropdownItem>
                    <DropdownItem>New Question</DropdownItem>
                </DropdownMenu>
            </Dropdown>
            <QuestionPopUp questions={questions} 
                           toggle={()=>setQuestionPopupOpen(!questionPopupOpen)} 
                           isOpen={questionPopupOpen} 
                           onSelection={e=>alert(JSON.stringify(e))}/>
        </>
    );
}