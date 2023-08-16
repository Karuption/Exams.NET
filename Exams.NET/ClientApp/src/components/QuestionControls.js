import {Card, CardBody, Modal, ModalBody} from "reactstrap";
import {useEffect} from "react";

export function QuestionPopUp ({questions = [], onSelection=(e)=>{}, isOpen, toggle}){
    let questionCount = questions.length

    useEffect(() => {
        questionCount = questions.length;
    }, [questions]);

    return (
        <Modal isOpen={isOpen} toggle={toggle}>
            <ModalBody>
                { questionCount > 0 ?
                    <QuestionSelector questions={questions} onCardClick={e=>{toggle();onSelection(e)}} />
                    : <h1 className={'text-danger'}>No Questions to select</h1>
                }
            </ModalBody>
        </Modal>
    );
}

export function QuestionSelector ({questions = [], onCardClick}) {
    return (
        questions.map(question => (
            <Card key={question.testQuestionId} onClick={()=>onCardClick(question)}>
                <CardBody>
                    <QuestionTextView question={question}/>
                </CardBody>
            </Card>
        ))
    );
}

export function QuestionTextView({question={}}){
    return (
        <>
            <h5>{question.prompt}</h5>
            {
                question.choices
                    ?
                    <ul>
                        {
                            question.choices.map(choice => (
                                <li key={choice.id}>{choice.description}</li>
                            ))
                        }
                    </ul>
                    :<></>
            }
        </>);
}