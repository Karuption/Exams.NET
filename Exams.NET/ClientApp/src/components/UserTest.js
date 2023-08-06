import authService from "./api-authorization/AuthorizeService";
import {useEffect, useState} from "react";
import {Button, Card, CardBody, CardHeader, CardSubtitle, Col, Form, FormGroup, Input, Label} from "reactstrap";


export default function UserTest({testId}) {
    const [loading, setLoading] = useState(true);
    const [test, setTest] = useState({testTitle:"", testDescription:"",testId:"", problems:[]});
    
    useEffect(()=> {populateTest(testId)}, []);
    
    let content = loading ?
        <p><em>Loading...</em></p>
        : <UserTestView userTest={test}/>;
    
    return (
        <div>
            {content}
        </div>
    );
    
    async function populateTest(id) {
        const token = await authService.getAccessToken();
        await fetch(`api/test/${id}`, {
            headers: !token ? {} : { 'Authorization': `Bearer ${token}`, 'Accept': 'application/json' },
        }).then(res => {
            if(!res.ok) {
                console.log(res);
                setLoading( false);
            }
            else
                return res.json();
        })
            .then(data => {setTest(data); setLoading(false);});
    }
}

function UserTestView ({userTest}) {
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
                                <Form>
                                    <FormGroup check={true}>
                                        {
                                            problem.choices.map(choice =>(
                                                <div>
                                                    <Input id={choice.id}
                                                           className={"form-check-input"}
                                                           name={"answer"}
                                                           key={choice.id}
                                                           type={"radio"}/>
                                                    <Label className={"form-check-label"} 
                                                           for={choice.id} 
                                                           check={true}>{choice.description}</Label>
                                                </div>
                                            ))}
                                        </FormGroup>
                                    </Form>
                                </CardBody>
                            : <CardBody className={"px-2"}>
                                <Form>
                                    <FormGroup floating={true}>
                                        <Input name={"answer"} 
                                               type={"textarea"} />
                                        <Label for={"answer"}>Answer</Label>
                                    </FormGroup>
                                </Form>
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