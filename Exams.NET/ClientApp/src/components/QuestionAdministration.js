import authService from "./api-authorization/AuthorizeService";
import React, {useEffect, useState} from "react";
import {Button, CloseButton, Modal, ModalBody, ModalHeader, Spinner} from "reactstrap";
import QuestionForm from "./QuestionForm";
import {
    ItemAdministration, ItemAdministrationBody,
    ItemAdministrationBodyEntry, ItemAdministrationHeader, ItemAdministrationHeaderEntry,
    ItemAdministrationRow, ItemAdministrationRowActions, ItemAdministrationSubtitle, ItemAdministrationTable,
    ItemAdministrationTableBody, ItemAdministrationTableHeader
} from "./ItemAdministration";

export function QuestionAdministration() {
    const [questions, setQuestions] = useState([]);
    const [qModal, setQModal] = useState(false);
    const [modalHeader, setModalHeader] = useState("");
    const [loading, setLoading] = useState(true);
    const [editQuestion, setEditQuestion] = useState({});
    
    useEffect(() => {
            GetAllQuestions();
    }, []);

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
                            <QuestionForm editQuestion={editQuestion} callback={e=>{setEditQuestion(e);setQModal(!qModal);}}/>
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
                                            <ItemAdministrationBodyEntry>{q.testId > 0 ? q.testId : "N/A"}</ItemAdministrationBodyEntry>
                                            <ItemAdministrationRowActions>
                                                <Button className={'btn btn-primary d-flex align-items-center py-1 me-0'}
                                                        onClick={_=>{setEditQuestion(q);setModalHeader("Edit");setQModal(true)}}>edit</Button>
                                                <CloseButton style={{fontSize: 23}} onClick={_=>deleteQuestion(q)}/>
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