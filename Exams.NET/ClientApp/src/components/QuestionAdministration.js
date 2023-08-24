import authService from "./api-authorization/AuthorizeService";
import React, {Suspense, useCallback, useEffect, useState} from "react";
import {Button, CloseButton, Modal, ModalBody, ModalHeader, Spinner} from "reactstrap";
import QuestionForm from "./QuestionForm";
import {
    ItemAdministration, ItemAdministrationBody,
    ItemAdministrationBodyEntry, ItemAdministrationHeader, ItemAdministrationHeaderEntry,
    ItemAdministrationRow, ItemAdministrationRowActions, ItemAdministrationSubtitle, ItemAdministrationTable,
    ItemAdministrationTableBody, ItemAdministrationTableHeader
} from "./ItemAdministration";
import {FaEdit, FaPaperclip, FaSyncAlt} from "react-icons/fa";
import {FaL, FaRotateLeft, FaUpRightFromSquare, FaX} from "react-icons/fa6";
import {TestSelector} from "../TestSelector";

export function QuestionAdministration() {
    const [questions, setQuestions] = useState([]);
    const [qModal, setQModal] = useState(false);
    const [modalHeader, setModalHeader] = useState("");
    const [loading, setLoading] = useState(true);
    const [editQuestion, setEditQuestion] = useState({});
    const [tModal, setTModal] = useState(false);
    const [tView, setTView] = useState(false);
    
    useEffect(() => {
            GetAllQuestions();
    }, []);

    const questionCallback = useCallback(async (question, test) => {
        question.testId = test.testId;
        updateQuestion(question);
    }, [updateQuestion]);

    async function deleteQuestion(question) {
        const token = await authService.getAccessToken();
        setLoading(true);
        await fetch(`api/admin/Question/${question.testQuestionId}`, {
            method : "DELETE",
            headers: !token?{}:{'Authorization' : `Bearer ${token}`}
        })
            .then(res => res.ok&&GetAllQuestions())
            .catch(err => console.log(err));
    }

    return (
        <div>
            <ItemAdministration>
                <ItemAdministrationHeader>Question Administration</ItemAdministrationHeader>
                <ItemAdministrationSubtitle>This is for the high level management of test questions.</ItemAdministrationSubtitle>
                <ItemAdministrationBody>
                    <button className={"btn btn-primary"} onClick={_ => {
                        setModalHeader("Create New Question");
                        setQModal(true);
                        setEditQuestion(null);
                    }}>Create New Test Question
                    </button>
                    <Modal isOpen={qModal} toggle={_ => setQModal(n => !n)}>
                        <ModalHeader>{modalHeader}</ModalHeader>
                        <ModalBody>
                            <QuestionForm editQuestion={editQuestion} callback={e=>{setEditQuestion(e);setQModal(!qModal);GetAllQuestions()}}/>
                        </ModalBody>
                    </Modal>
                    <Modal isOpen={tModal} toggle={_ => setTModal(n => !n)}>
                        <ModalHeader>Test Selection</ModalHeader>
                        <ModalBody>
                            <Suspense fallback={<Spinner />}>
                                <TestSelector testId={editQuestion?.testId??0} viewOnly={tView}
                                              callback={t=>{(!tView)&&questionCallback(editQuestion,t);setTModal(false)}} />
                            </Suspense>
                        </ModalBody>
                    </Modal>
                    <ItemAdministrationTable loading={loading} fallbackColumnSpan={"5"}>
                        <ItemAdministrationTableHeader>
                            <ItemAdministrationRow>
                                <ItemAdministrationHeaderEntry>#</ItemAdministrationHeaderEntry>
                                <ItemAdministrationHeaderEntry>Type</ItemAdministrationHeaderEntry>
                                <ItemAdministrationHeaderEntry>Prompt</ItemAdministrationHeaderEntry>
                                <ItemAdministrationHeaderEntry>Test</ItemAdministrationHeaderEntry>
                                <ItemAdministrationHeaderEntry className={'d-flex align-items-end justify-content-end pe-4'}>Actions</ItemAdministrationHeaderEntry>
                            </ItemAdministrationRow>
                        </ItemAdministrationTableHeader>
                        <ItemAdministrationTableBody >
                            {
                                questions.map((q, i) => (
                                    <ItemAdministrationRow key={i}>
                                        <ItemAdministrationBodyEntry>{i + 1}</ItemAdministrationBodyEntry>
                                        <ItemAdministrationBodyEntry>{q.Type === "MultipleChoice" ? "Multiple Choice" : "Free Answer"}</ItemAdministrationBodyEntry>
                                        <ItemAdministrationBodyEntry>{q.prompt}</ItemAdministrationBodyEntry>
                                        <ItemAdministrationBodyEntry className={''}>
                                            {
                                                q.testId > 0 
                                                    ? <Button className={'btn btn-link d-flex align-items-center py-1 me-0 px-0'} 
                                                              onClick={_=>{setTView(true);setEditQuestion(q);setTModal(true)}}>
                                                        <FaUpRightFromSquare fontSize={23} aria-label={"Go to Test Page"} />
                                                    </Button>
                                                    : <Button className={'btn btn-link d-flex align-items-center py-1 me-0 px-0'} 
                                                              onClick={x=>x}>
                                                        <FaPaperclip fontSize={23} aria-label={"Assign Test"} 
                                                                     onClick={_=>attachModal(q)}/>
                                                    </Button>
                                            }
                                        </ItemAdministrationBodyEntry>
                                        <ItemAdministrationRowActions>
                                            {
                                                q.testId > 0
                                                    ? <Button className={'btn btn-link d-flex align-items-center py-1 px-0'}
                                                              onClick={_=>{delete q.testId;updateQuestion();}}>
                                                        <FaRotateLeft fontSize={23} aria-label={"Remove Question from test"}/>
                                                    </Button>
                                                    : <></>
                                            }
                                            <Button className={'btn btn-link d-flex align-items-center py-1 px-0'}
                                                    onClick={_=>attachModal(q)}>
                                                <FaSyncAlt fontSize={23} aria-label={"Re-Assign Question"}/>
                                            </Button>
                                            <Button className={'btn btn-link d-flex align-items-center py-1 px-0'}
                                                    onClick={_=>{setEditQuestion(q);setModalHeader("Edit");setQModal(true)}}>
                                                <FaEdit fontSize={23} aria-label={"Edit Question"}/>
                                            </Button>
                                            <Button className={'btn btn-link d-flex align-items-center py-1 ps-0'}
                                                    onClick={_=>deleteQuestion(q)}>
                                                <FaX fontSize={23} aria-label={"Delete Question"}/>
                                            </Button>
                                        </ItemAdministrationRowActions>
                                    </ItemAdministrationRow>
                                ))
                            }
                        </ItemAdministrationTableBody>
                    </ItemAdministrationTable>
                </ItemAdministrationBody>
            </ItemAdministration>
        </div>
    );
    
    function attachModal(q){
        setTView(false);
        setTModal(true);
        setEditQuestion(q);
    }

    async function updateQuestion(q) {
        const token = await authService.getAccessToken();
        setLoading(true);
        const submitQuestion = {...q};
        delete submitQuestion.Type;
        await fetch(`api/admin/Question/${q.choices?'MultipleChoice':'FreeForm'}`, {
            method: 'PUT',
            headers: !token ? {} : {
                'Authorization': `Bearer ${token}`, 'Accept': 'application/json',
                'Content-Type': 'application/json'
            },
            body: JSON.stringify(submitQuestion),
        }).catch(e=>console.log(e))
          .then(_ => {setLoading(false);GetAllQuestions();;});
    }
    
    async function GetAllQuestions() {
        const token = await authService.getAccessToken();
        await fetch(`api/admin/Question`, {
            headers: !token ? {} : {'Authorization': `Bearer ${token}`, 'Accept': 'application/json'},
        }).then(res => {
            if (!res.ok)
                console.log(res);
            else
                return res.json();
        })
            .then(data => {setQuestions(data);setLoading(false);});
    }
}