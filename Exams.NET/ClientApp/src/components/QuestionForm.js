import { Button, Col, Form, FormGroup, Input, Label, Row } from "reactstrap";
import {useEffect, useState} from "react";
import authService from "./api-authorization/AuthorizeService";

const MultipleChoice = ({choices, handleOptionChange}) => {
    const alphabet = Array.from({length: 26}, (_,i) => String.fromCharCode(65 + i));
    const [allowAdd, setAllowAdd] = useState(true);
    
    return (
        <div>
            { choices.map((option, index) => (
                <Row key={index}>
                    <Col md={2}>
                        <FormGroup floating={true}>
                            <Input id={"choicePointValue "+index+1} 
                                   name={"choicePointValue"} 
                                   type={"number"} 
                                   min={0}/>
                            <Label for={"choicePointValue "+index+1}>{alphabet[index]} Point Value</Label>
                        </FormGroup>
                    </Col>
                    <Col>
                        <FormGroup floating={true}>
                            <Input
                                id={"Description "+index+1}
                                name={"description"}
                                onChange={(e)=>{handleOptionChange(e, index)}}
                                type="text"
                                bsSize="sm" />
                            <Label for={"Description "+index+1}>Choice {alphabet[index]}</Label>
                        </FormGroup>
                    </Col>    
                </Row>
            ))}
            <Button disabled={!allowAdd} onClick={(e)=>handleOptionChange(null, choices.length)} className={"btn btn-primary"}>Add Choice</Button>
        </div>
    );
};

const FreeFormQuestion = ({answer, handleAnswerChange}) => {
    return (
        <FormGroup floating={true}>
            <Input
                id="answer"
                name="answer"
                type="text"
                bsSize="sm"
                value={answer}
                onChange={handleAnswerChange}
            />
            <Label for="answer">Answer</Label>
        </FormGroup>
    );
};

export default function QuestionForm() {
    const [submittable, setSubmittable] = useState(true);
    const [question, setQuestion] = useState({prompt:"", totalPointValue:0});
    const [questionType, setQuestionType] = useState("Multiple Choice");
    const [choices, setChoices] = useState([{choicePointValue: 0,description:""}]);
    const [ffAnswer, setFFAnswer] = useState("");
    
    const handleQuestionTypeChange = (event) => {
        event.preventDefault();
        const type = event.target.value;
        setQuestionType(type);
    }
    
    const handleFormChanges = (event) => {
        event.preventDefault();
        const {name, value} = event.target;
        setQuestion({...question, [name]: value});
    }

    const handleOptionChange = (event, index) => {
        const newChoices = [...choices];
        if(event) {
            const {name, value} = event.target;
            newChoices[index] = {...newChoices[index], [name]: value};
        } else 
            newChoices.push({choicePointValue:0,description:""});
        setChoices(newChoices);
    }

    const handleFFChange = (event) => {
        event.preventDefault();
        setFFAnswer(event.target.value);
    }

    const handleSubmit = async (event)=>{
        event.preventDefault();
        let submitQuestion = question
        if(questionType === "Multiple Choice"){
            submitQuestion = {...submitQuestion, choices: choices}
        } else {
            submitQuestion = {...submitQuestion, answer: ffAnswer}
        }
        if(!submitQuestion.TestQuestionId)
            await submitNewQuestion(submitQuestion);
        else 
            await submitEditQuestion(submitQuestion);
    }
    const submitNewQuestion= async (prop) => {
        const token = await authService.getAccessToken();
        await fetch('api/admin/Question', {
            method : "POST",
            headers : !token? {} : {'Authorization' : `Bearer ${token}`, 'Content-Type' : 'application/json'},
            body : JSON.stringify({...prop})
        })
            .then(response => response.json())
            .then(data => console.log(data))
            .catch(err => console.log(err));
    }
    
    const submitEditQuestion = async (prop) => {
        const token = await authService.getAccessToken();
        await fetch(`api/admin/Question/${prop.testId}`, {
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

    return(
        <Form onSubmit={handleSubmit}>
            <FormGroup floating={true}>
                <Input id={"totalPointValue"}
                       name={"totalPointValue"}
                       min={0}
                       type={"number"}
                       bsSize={"sm"}
                       onChange={handleFormChanges}/>
                <Label for={"totalPointValue"} >
                    Total Point Value
                </Label>
            </FormGroup>

            <FormGroup floating={true}>
                <Input id={"questionType"}
                       name={"questionType"}
                       className={"mb-3"}
                       type={"select"}
                       onChange={handleQuestionTypeChange}>
                    <option>Multiple Choice</option>
                    <option>Free Answer</option>
                </Input>
                <Label for={"questionType"}>Question Type</Label>
            </FormGroup>
            <FormGroup floating={true}>
                <Input id={"prompt"}
                       name={"prompt"}  
                       value={question.prompt}
                       type={"textarea"}
                       rows={4}
                       bsSize={"sm"}
                       style={{resize: 'none', height: '7rem'}}
                       onChange={handleFormChanges}/>
                <Label for={"prompt"}>
                    Question Prompt
                </Label>
            </FormGroup>
            {questionType==="Multiple Choice"
                ? <MultipleChoice choices={choices} handleOptionChange={handleOptionChange}/>
                : <FreeFormQuestion answer={ffAnswer} handleAnswerChange={handleFFChange}/>}
            <Button className={"btn btn-primary"} disabled={!submittable}>OK</Button>
        </Form>
    );
}