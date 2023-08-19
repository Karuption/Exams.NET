import React, { useEffect, useState } from "react";
import {
    Button,
    Card,
    CardBody,
    CardHeader,
    Label,
    CloseButton,
    Dropdown,
    DropdownItem,
    DropdownMenu,
    DropdownToggle,
    Form,
    FormFeedback,
    FormGroup,
    Input, Modal, ModalBody,
} from "reactstrap";
import authService from "./api-authorization/AuthorizeService";
import { QuestionPopUp, QuestionTextView } from "./QuestionControls";
import QuestionForm from "./QuestionForm";

export default function TestForm( { ParentCallback , editTest } ) {
    const [testForm, setTestForm] = useState(editTest);
    const [testFormValidation, setTestFormValidation] = useState({isValidTitle: true, isValidDescription: true, titleError:"", descriptionError:""});
    const [submitBlock, setSubmitBlock] = useState(true);
    const [editing,setEditing] = useState(Object.keys(editTest).length !== 0);
    const [problems, setProblems] = useState([]);
    
  // Test form editing, if its null/undefined/empty, then we are editing
    useEffect(() => {
        if (editTest === null || Object.keys(editTest).length === 0) {
            setTestForm(_ => ({ testTitle: "", testDescription: "", problems: [] }));
            setEditing(false);
        } else {
            setEditing(true);
            setTestForm(_ => ({...editTest}));
        }
      setProblems(testForm.problems);
    }, [editTest]);

  // set submit block and validate the test
    useEffect(() => {
        if((editing && testForm === editTest) || testForm.testTitle === ""){
            setTestFormValidation({...testFormValidation, isValidDescription: false, titleError: "This cannot be empty"});
            return setSubmitBlock(true);
        }
        setSubmitBlock(false);
    },[testForm,editTest,editing,problems]);
  

    const handleSubmit = async (event) => {
        event.preventDefault();
        testForm.problems = problems;
        if (!editing) 
          await submitNewTest(testForm);
        else 
          await submitEditedTest(testForm);
        await ParentCallback();
    }

    const handleFormChanges = (event) => {
        event.preventDefault();
        const { name, value } = event.target;
        setTestForm({ ...testForm, [name]: value });
    }

    const submitNewTest = async (prop) => {
        const token = await authService.getAccessToken();
        await fetch(`api/admin/Test`, {
            method: "POST",
            headers : !token? {} : {'Authorization' : `Bearer ${token}`, 'Content-Type' : 'application/json'},
            body: JSON.stringify({ ...prop })
        })
            .then(response => {
                if(response.ok)
                    return response.json()
                else
                    throw Error (`Unable to send response. ${JSON.stringify(response)}`);
            }).then(data => console.log(data))
            .catch(err => console.log(err));
    }

    const submitEditedTest = async (prop) => {
    const token = await authService.getAccessToken();
    await fetch(`api/admin/Test/${prop.testId}`, {
      method: "PUT",
            headers : !token? {} : {'Authorization' : `Bearer ${token}`, 'Content-Type' : 'application/json'},
      body: JSON.stringify({ ...prop })
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
                       onChange={handleFormChanges}
                       type={"textarea"}
                       value={testForm.testDescription} />
                <Label for={"testDescription"}>Test Description</Label>
            </FormGroup>
            {
                problems.length > 0 && 
                <>
                    <div className={'d-flex justify-content-center my-2'}>
                        <h5 className={'my-0'}>Test Questions</h5>
                    </div>
                        { 
                            problems.map((problem, index) => (
                                <Card className={'my-3'} key={index}>
                                    <CloseButton className={'position-absolute top-0 end-0 me-2 mt-2'} 
                                                 onClick={_=>removeQuestion(problem,index)}/>
                                    <CardHeader>{problem.choice?'Multiple choice':'Free Answer'}</CardHeader>
                                    <CardBody>
                                        <QuestionTextView question={problem} />
                                    </CardBody>
                                </Card> 
                            ))
                        }
                </>
            }
            <FormGroup>
                <QuestionDropdownMenu questions={problems} testId={testForm.testId} onQuestionSelection={addQuestion}/>
            </FormGroup>
            <Button className={"btn btn-primary text-center"} disabled={submitBlock}>Submit</Button>
        </Form>
    );
    
    function removeQuestion(problem, index) {
        const newProblems = [...problems];
        newProblems.splice(index, 1);
        setProblems(_ => newProblems);
        testForm.problems = problems;
    }
    
    function addQuestion(problem) {
        setProblems(problems=>[...problems, problem]);
        testForm.problems = problems;
    }
}

function QuestionCreationPopUp({ onQuestionCreation = x=>x, toggle, isOpen, testId}) {
    return (
        <Modal isOpen={isOpen} toggle={toggle}>
            <ModalBody>
                <QuestionForm testId={testId} callback={(x) => {
                    toggle();
                    onQuestionCreation(x);
                }} />
            </ModalBody>
        </Modal>
    );
}
function QuestionDropdownMenu({questions = [], onQuestionSelection, testId}) {
    const [open, setOpen] = useState(false);
    const [questionPopupOpen, setQuestionPopupOpen] = useState(false);
    const [qCreation, setQCreation] = useState(false);
    const [allQuestions, setAllQuestions] = useState([...questions]);

    useEffect(async () => {
        await getAllQuestions();
    }, []);

    async function getAllQuestions() {
        const token = await authService.getAccessToken();
        await fetch(`api/admin/Question`, {
            headers: !token ? {} : {'Authorization': `Bearer ${token}`, 'Accept': 'application/json'},
        }).then(res => {
            if (!res.ok)
                console.log(res);
            else
                return res.json();
        })
            .then(data => setAllQuestions(data));
    }

    return (
        <>
            <Dropdown isOpen={open} toggle={() => setOpen(!open)} direction={'down'}>
                <DropdownToggle caret={true} color={'primary'}>Add a test question</DropdownToggle>
                <DropdownMenu end={true}>
                    <DropdownItem disabled={allQuestions.length < 1} onClick={() => setQuestionPopupOpen(true)}>Clone
                                                                                                                existing</DropdownItem>
                    <DropdownItem onClick={_ => setQCreation(!qCreation)}>Create New Question</DropdownItem>
                </DropdownMenu>
            </Dropdown>
            <QuestionCreationPopUp testId={testId} onQuestionCreation={x => onQuestionSelection(x)}
                                   toggle={_ => setQCreation(!qCreation)} isOpen={qCreation} />
            <QuestionPopUp questions={allQuestions}
                           toggle={() => setQuestionPopupOpen(!questionPopupOpen)}
                           isOpen={questionPopupOpen}
                           onSelection={x=>onQuestionSelection(x)} />
        </>
    );
}