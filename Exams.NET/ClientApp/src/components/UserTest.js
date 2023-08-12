import authService from "./api-authorization/AuthorizeService";
import {useEffect, useState} from "react";
import {Button, Card, CardBody, CardHeader, CardSubtitle, Form, FormGroup, Input, Label} from "reactstrap";

export default function UserTest({testId}) {
    const [loading, setLoading] = useState(true);
    const [test, setTest] = useState({testTitle:"", testDescription:"",testId:"", problems:[]});
    const [userAnswers, setUserAnswers] = useState([]);
    
    useEffect(()=> {populateTest(testId);getUserAnswersByTestId(testId)}, []);
    useEffect(()=>{ 
        if(test.problems && test.problems.length > 0 && 
            userAnswers && userAnswers.length > 0)
            setLoading(false);
        }, [test,userAnswers])
    const answerChange = (index, answer) => {
        const updatedAnswers = [...userAnswers];

        updatedAnswers[index].answer = answer;
        updatedAnswers[index].id = test.problems[index].testQuestionId
        setUserAnswers(updatedAnswers);
    }
    
    let content = loading ?
        <p><em>Loading...</em></p>
        : <UserTestView userTest={test} userAnswers={userAnswers} answerChange={answerChange}/>;
    
    return content;
    
    async function populateTest(id) {
        const token = await authService.getAccessToken();
        await fetch(`api/test/${id}`, {
            headers: !token ? {} : { 'Authorization': `Bearer ${token}`, 'Accept': 'application/json' },
        }).then(res => {
            if(!res.ok) 
                console.log(res);
            else
                return res.json();
        })
            .then(data => setTest(data));
    }
    
    async function getUserAnswersByTestId(testId){
        const token = await authService.getAccessToken();
        await fetch(`api/UserAnswer/${testId}`, {
            headers: !token ? {} : { 'Authorization': `Bearer ${token}`, 'Accept': 'application/json' },
        }).then(res => {
            if(!res.ok) {
                console.log(res);
            }
            return res.json();
        })
            .then(data => setUserAnswers(data));
    }

    async function putUserAnswer(userAnswer){
        const token = await authService.getAccessToken();
        await fetch(`api/UserAnswer/${userAnswer}`, {
            method: 'PUT',
            headers: !token ? {} : { 'Authorization': `Bearer ${token}`, 'Accept': 'application/json' },
        }).then(res => {
            if(!res.ok) {
                console.log(res);
            }
            else
                return res.json();
        })
            .then(data => setTest(data));
    }
}

function UserTestView ({userTest, userAnswers, answerChange}) {
    return(
        <div>
            <h1>{userTest.testTitle}</h1>
            <p>{userTest.testDescription}</p>
            {userTest.problems.map((problem,index) =>
                <Card className={"my-3"} body={true} key={index}>
                    <CardHeader className={"d-flex justify-content-between mx-0 px-0 mb-3"}>
                        <div>{index+1}.</div>
                        <div>{problem.totalPointValue} Points</div>
                    </CardHeader>
                    <CardSubtitle className={"mx-2"}>{problem.prompt}</CardSubtitle>
                    {
                        problem.Type === "MultipleChoice" 
                            ? <CardBody>
                                <MultipleChoiceAnswerForm 
                                    choices={problem.choices} 
                                    questionId={index}
                                    onChange={answerChange}
                                    selected={userAnswers[index].answer}/>
                              </CardBody>
                            : <CardBody className={"px-2"}>
                                <FreeformAnswerForm answer={userAnswers[index].answer} 
                                                    questionId={index}/>
                              </CardBody>
                    }
                </Card>
            )}
            <div className={"d-flex flex-row-reverse px-3"}>
                <Button className={"btn-primary"}>Submit</Button>
            </div>
        </div>
    );
}

function MultipleChoiceAnswerForm ({choices, selected = "", questionId, onChange}) {
    return (
        <Form>
            <FormGroup check={true}>
                {
                    choices.map((choice) =>(
                        <div key={choice.id}>
                            <Input id={choice.id}
                                   className={"form-check-input"}
                                   checked={choice.id === selected}
                                   onChange={_=>onChange(questionId, choice.id)}
                                   type={"radio"} />
                            <Label className={"form-check-label"}
                                   for={choice.id}
                                   check={true}>{choice.description}</Label>
                        </div>
                    ))}
            </FormGroup>
        </Form>
    );
}

function FreeformAnswerForm ({answer = "", questionId, onChange}) {
    const [freeformAnswer, setFreeformAnswer] = useState(answer);
    
    return (
        <Form>
            <FormGroup floating={true}>
                <Input id={"answer"}
                       type={"textarea"}
                       value={freeformAnswer} 
                       onChange={event => setFreeformAnswer(event.target.value)}
                       onBlur={_=>onChange(questionId, freeformAnswer)} />
                <Label for={"answer"}>Answer</Label>
            </FormGroup>
        </Form>
    );
}