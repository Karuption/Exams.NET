import {Button, Col, Form, FormFeedback, FormGroup, Input, Label, Row} from "reactstrap";
import {useEffect, useState} from "react";
import authService from "./api-authorization/AuthorizeService";

const MultipleChoice = ({ choices, handleChoiceChange, choiceValidity }) => {
    const alphabet = Array.from({length: 26}, (_,i) => String.fromCharCode(65 + i));
    const [allowAdd, setAllowAdd] = useState(true);
    
    useEffect(()=>{
        if(choiceValidity.some(x=>!x.isPointValid || !x.isDescriptionValid))
            return setAllowAdd(false);
        setAllowAdd(true);
    }, [choiceValidity])
    
    return (
        <div>
            { choices.map((choice, index) => (
                <Row key={index}>
                    <Col md={2}>
                        <FormGroup floating={true}>
                            <Input id={"choicePointValue " + index + 1}
                                   name={"choicePointValue"} 
                                   type={"number"} 
                                   value={choice.choicePointValue}
                                   onChange={(e)=>handleChoiceChange(e, index)}
                                   valid={choiceValidity[index].isPointValid} 
                                   invalid={!choiceValidity[index].isPointValid}/>
                            <Label for={"choicePointValue " + index + 1}>{alphabet[index]} Point Value</Label>
                            <FormFeedback valid={false}>{choiceValidity[index].pointErrorMessage}</FormFeedback>
                        </FormGroup>
                    </Col>
                    <Col>
                        <FormGroup floating={true}>
                            <Input
                                id={"Description " + index + 1}
                                name={"description"}
                                onChange={(e)=>{handleChoiceChange(e, index)}}
                                type="text"
                                bsSize="sm" 
                                invalid={!choiceValidity[index].isDescriptionValid}/>
                            <Label for={"Description " + index + 1}>Choice {alphabet[index]}</Label>
                            <FormFeedback valid={false}>{choiceValidity[index].descriptionError}</FormFeedback>
                        </FormGroup>
                    </Col>    
                </Row>
            ))}
            <Button disabled={!allowAdd} 
                    onClick={()=>handleChoiceChange(null, choices.length)} 
                    className={"btn btn-primary"}>Add Choice</Button>
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
                invalid={!(answer!=="")}
                onChange={handleAnswerChange} />
            <Label for="answer">Answer</Label>
            <FormFeedback valid={false}>This cannot be empty</FormFeedback>
        </FormGroup>
    );
};

export default function QuestionForm( { editQuestion = {} }) {
    const [submittable, setSubmittable] = useState(true);
    const questionTypes = Object.freeze({
        "MultipleChoice": "Multiple Choice",
        "FreeForm": "Free Form"
    });
    const [question, setQuestion] = useState({prompt:"", totalPointValue:0, ...editQuestion});
    const [questionValidity, setQuestionValidity] = useState({isPromptValid: true, isPointValueValid: true, promptError: "", pointValueError: ""});
    const [questionType, setQuestionType] = useState(questionTypes.MultipleChoice);
    const [choices, setChoices] = useState([{choicePointValue: 0,description:""}]);
    const [ffAnswer, setFFAnswer] = useState("");
    const [multipleChoiceValidity, setMultipleChoiceValidity] = useState([{isPromptValid:true,isPointValueValid:true,descriptionError:"",pointValueError:""}]);

    useEffect(() => {
        setQuestion({...editQuestion});
    }, [editQuestion]);
    
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

    // option validity
    useEffect(() => {
        const newValidity = [...multipleChoiceValidity];
        choices.map((choice, index)=> {
            newValidity[index] = {...newValidity[index], isPointValid: true, isDescriptionValid: true};
            
            if(choice.choicePointValue < 0)
                newValidity[index] = {
                    ...newValidity[index],
                    isPointValid: false,
                    pointErrorMessage: "Must be greater than 0"
                };
            if(choice.choicePointValue > question.totalPointValue && question.totalPointValue >= 0)
                newValidity[index] = {
                    ...newValidity[index],
                    isPointValid: false,
                    pointErrorMessage: `Must be less than or equal to the total amount of question points: ${question.totalPointValue}`
                };
            
            newValidity[index].isDescriptionValid = !(choice.description === "");
        });
        setMultipleChoiceValidity(newValidity);
    },[choices, question.totalPointValue]);

    // Just deals with the changes validity is handled by an effect
    const handleMultipleChoiceChanges = (event, index) => {
        const newChoices = [...choices];
        if(event) {
            let {name, value} = event.target;
            newChoices[index] = {...newChoices[index], [name]: value};
        } else {
            const newValidity = [...multipleChoiceValidity];
            newChoices.push({choicePointValue: 0, description: ""});
            newValidity.push({isPromptValid:true,isPointValueValid:true,descriptionError:"",pointValueError:""});
            setMultipleChoiceValidity(newValidity);
        }
        setChoices(newChoices);
    }

    const handleFreeFormChanges = (event) => {
        event.preventDefault();
        setFFAnswer(event.target.value);
    }

    // question validation
    useEffect(() => {
        const newQuestionValidity = {...questionValidity , isPromptValid: true, isPointValueValid: true};
        if(questionType === questionTypes.MultipleChoice && choices.length > 1){
            newQuestionValidity.isPointValueValid = choices.some(x=>x.choicePointValue ===question.totalPointValue);
            newQuestionValidity.pointValueError = "Multiple Choice questions must have at least one answer that will grant full credit";
        }
        if(question.totalPointValue < 0) {
            newQuestionValidity.isPointValueValid = false;
            newQuestionValidity.pointValueError = "Point values must be non-negative";
        }
        
        if(!(question.prompt !== "")){
            newQuestionValidity.isPromptValid = false;
            newQuestionValidity.promptError = "This cannot be empty";
        }
        
        setQuestionValidity(newQuestionValidity);
    }, [question,questionType,choices]);

    // submittable 
    useEffect(() => {
        let submittable;
        
        if(questionType === questionType.MultipleChoice)
            submittable = multipleChoiceValidity.every(x=>x.isPointValid&&x.isDescriptionValid) && 
                choices.some(x=>x.choicePointValue === question.totalPointValue);
        else 
            submittable = !(ffAnswer !== "");
        
        submittable = submittable 
                && questionValidity.isPromptValid 
                && questionValidity.isPointValueValid 
                && (editQuestion !== question);
        setSubmittable(submittable);
    }, [questionValidity, questionType, multipleChoiceValidity]);

    const handleSubmit = async (event)=>{
        event.preventDefault();
        let submitQuestion = question
        if(questionType === questionType.MultipleChoice){
            submitQuestion = {...submitQuestion, choices: choices}
        } else {
            submitQuestion = {...submitQuestion, answer: ffAnswer}
        }
        if(submitQuestion.TestQuestionId)
            await submitEditQuestion(submitQuestion);
        else 
            await submitNewQuestion(submitQuestion);
    }
    const submitNewQuestion = async (prop) => {
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
                       value={question.totalPointValue}
                       onChange={handleFormChanges}
                       invalid={!questionValidity.isPointValueValid} />
                <Label for={"totalPointValue"} >Total Point Value</Label>
                <FormFeedback valid={false}>{questionValidity.pointValueError}</FormFeedback>
            </FormGroup>

            <FormGroup floating={true}>
                <Input id={"questionType"}
                       name={"questionType"}
                       className={"mb-3"}
                       type={"select"}
                       value={questionType}
                       onChange={handleQuestionTypeChange}>
                    {Object.values(questionTypes).map((qType) =>
                        (<option key={qType} value={qType}>{qType}</option>)
                    )}
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
                       invalid={!questionValidity.isPromptValid}
                       style={{resize: 'none', height: '7rem'}}
                       onChange={handleFormChanges}/>
                <Label for={"prompt"}>Question Prompt</Label>
                <FormFeedback valid={false}>{questionValidity.promptError}</FormFeedback>
            </FormGroup>
            {questionType===questionTypes.MultipleChoice
                ? <MultipleChoice choices={choices} handleChoiceChange={handleMultipleChoiceChanges} choiceValidity={multipleChoiceValidity}/>
                : <FreeFormQuestion answer={ffAnswer} handleAnswerChange={handleFreeFormChanges}/>}
            <Button className={"btn btn-primary"} disabled={!submittable}>OK</Button>
        </Form>
    );
}