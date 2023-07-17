import {Alert, Button, Col, Form, FormGroup, Input, Label, Row, UncontrolledAlert} from "reactstrap";
import {useEffect, useState} from "react";

const MultipleChoice = ({options, handleOptionChange}) => {
    const alphabet = Array.from({length: 26}, (_,i) => String.fromCharCode(65 + i));
    const [allowAdd, setAllowAdd] = useState(true);
    
    useEffect(()=>setAllowAdd(options.length<alphabet.length),[options]);
    
    return (
        <div>
            { options.map((option, index) => (
                <Row key={index}>
                    <Col md={2}>
                        <FormGroup floating={true}>
                            <Input id={"choicePointValue "+index+1} 
                                   name={"choicePointValue "+index+1} 
                                   type={"number"} 
                                   min={0}/>
                            <Label for={"choicePointValue "+index+1}>{alphabet[index]} Point Value</Label>
                        </FormGroup>
                    </Col>
                    <Col>
                        <FormGroup floating={true}>
                            <Input
                                id={"Description "+index+1}
                                name={"Description "+index+1}
                                onChange={(e)=>{handleOptionChange(e, index)}}
                                type="text"
                                bsSize="sm" />
                            <Label for={"Description "+index+1}>Choice {alphabet[index]}</Label>
                        </FormGroup>
                    </Col>    
                </Row>
            ))}
            <Button disabled={!allowAdd} onClick={(e)=>handleOptionChange(null, options.length)} className={"btn btn-primary"}>Add Choice</Button>
        </div>
    );
};

export default function QuestionForm() {
    const [submittable, setSubmittable] = useState(true);
    const [question, setQuestion] = useState({prompt:"", totalPointValue:0});
    const [questionType, setQuestionType] = useState("Multiple Choice");
    const [options, setOptions] = useState(['','']);
    
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
        const newOptions = [...options];
        if(event!==null)
            return setOptions(newOptions[index] = event.target.value)
        newOptions.push('');
        setOptions(newOptions);
    }

    const FreeForm = () => {
        return (
            <FormGroup floating={true}>
                <Input
                    id="answer"
                    name="answer"
                    type="text"
                    bsSize="sm"
                />
                <Label for="answer">Answer</Label>
            </FormGroup>
        );
    };


    return(
        <Form>
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
                ? <MultipleChoice options={options} handleOptionChange={handleOptionChange}/>
                : <FreeForm/>}
            <Button className={"btn btn-primary"} disabled={!submittable}>OK</Button>
        </Form>
    );
}