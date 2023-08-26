import React, { Suspense, useEffect, useState } from "react";
import authService from "./api-authorization/AuthorizeService";
import {
   Button,
   CloseButton,
   Input,
   InputGroup,
   Modal,
   ModalBody,
   ModalHeader,
   Spinner,
} from "reactstrap";
import TestForm from "./TestAdminForm";
import { FaCheck, FaEdit, FaLink } from "react-icons/fa";
import { FaShareNodes, FaX, FaXmark } from "react-icons/fa6";

export default function TestAdmin() {
   const [tests, setTests] = useState([]);
   const [loading, setLoading] = useState(true);
   const [selectedTest, setSelectedTest] = useState({});
   const [testModal, setTestModal] = useState(false);
   const [headerText, setHeaderText] = useState("Create New Test");

   useEffect(() => {
      populateTests();
   }, []);

   let table = loading ? (
      <Spinner />
   ) : (
      <TestAdminTable
         tests={tests}
         editTest={(test) => {
            setSelectedTest(test);
            setTestModal(!testModal);
            setHeaderText(`Edit: ${selectedTest.testTitle}`);
         }}
         deleteTest={(id) => deleteTest(id)}
         loading={loading}
      />
   );

   return (
      <div>
         <h1 id='tableLabel'>Test Administration</h1>
         <p>This is for the high level administration of test.</p>
         <button
            className={"btn btn-primary"}
            onClick={() => {
               setSelectedTest({});
               setTestModal(!testModal);
            }}>
            Create New Test
         </button>
         <Modal isOpen={testModal} toggle={() => setTestModal((n) => !n)}>
            <ModalHeader>{headerText}</ModalHeader>
            <ModalBody>
               <TestForm
                  ParentCallback={() => {
                     setTestModal((n) => !n);
                     populateTests();
                  }}
                  editTest={selectedTest}
               />
            </ModalBody>
         </Modal>
         {table}
      </div>
   );

   async function populateTests() {
      const token = await authService.getAccessToken();
      await fetch("api/admin/test", {
         headers: !token
            ? {}
            : { Authorization: `Bearer ${token}`, Accept: "application/json" },
      })
         .then((res) => {
            if (!res.ok) {
               console.log(res);
               setLoading(false);
            } else return res.json();
         })
         .then((data) => {
            setTests((_) => [...data]);
            setLoading((_) => false);
         });
   }

   async function deleteTest(testId) {
      const token = await authService.getAccessToken();
      await fetch(`api/admin/Test/${testId}`, {
         method: "DELETE",
         headers: !token ? {} : { Authorization: `Bearer ${token}` },
      })
         .then((res) => res.ok && populateTests())
         .catch((err) => console.log(err));
   }
}

function TestAdminTable({ tests = [], editTest, deleteTest }) {
   const [modal, setModal] = useState(true);

   const ShareButton = ({ test = null }) => {
      const [copied, setCopied] = useState(false);
      const [shareId, setShareId] = useState(0);
      useEffect(() => {
         postShareRequest(test.testId);
      }, []);

      if (test === null) return <></>;

      async function postShareRequest(testId) {
         const token = await authService.getAccessToken();
         await fetch(`api/Share/${testId}`, {
            method: "POST",
            headers: !token
               ? {}
               : {
                    Authorization: `Bearer ${token}`,
                    "Content-Type": "application/json",
                 },
         })
            .then((res) => {
               if (!res.ok) {
                  console.log(res.body);
               } else return res.json();
            })
            .then(async (data) => await setShareId(data));
      }

      const shareableUrl = `${window.origin}/api/${test?.userId}/${test.testId}/${shareId}`;

      const copyToClipboard = () => {
         navigator.clipboard
            .writeText(shareableUrl)
            .then((_) => {
               console.log("no Copy");
               setCopied(true);
            })
            .catch((error) => {
               console.error("Error copying to clipboard:", error);
            });
      };

      return (
         <>
            <Modal isOpen={modal} toggle={(_) => setModal(!modal)}>
               <ModalBody>
                  <Suspense fallback={<Spinner />}>
                     <InputGroup onFocus={copyToClipboard}>
                        <Input
                           type={"text"}
                           contentEditable={false}
                           value={
                              shareId !== undefined
                                 ? shareableUrl
                                 : "Unable to share at this time"
                           }
                        />
                        <Button
                           color={
                              shareId === undefined
                                 ? "danger"
                                 : (copied && "success") || "secondary"
                           }>
                           {(copied && "Copied") || "Copy"}
                        </Button>
                     </InputGroup>
                  </Suspense>
               </ModalBody>
            </Modal>

            <Button
               color={"link"}
               className={"pt-0 pe-1"}
               aria-label={"share"}
               onClick={(_) => setModal(!modal)}>
               <FaShareNodes style={{ fontSize: 22 }} />
            </Button>
         </>
      );
   };
   return (
      <div>
         <table className='table table-striped'>
            <thead>
               <tr>
                  <th>Test name</th>
                  <th>Description</th>
                  <th>Created</th>
                  <th>Updated</th>
                  <th
                     className='d-flex align-items-center justify-content-end'
                     style={{ paddingRight: 12 }}>
                     Actions
                  </th>
               </tr>
            </thead>
            <tbody>
               {tests.length > 0 ? (
                  tests.map((test, index) => (
                     <TestTableEntry
                        test={test}
                        deleteTest={deleteTest}
                        editTest={editTest}
                        key={index}>
                        <ShareButton
                           test={test}
                           onClick={(_) => setModal(!modal)}
                        />
                        <Button
                           color='link'
                           className={"pt-0 pe-1"}
                           aria-label={"Edit"}
                           onClick={(_) => {
                              editTest(test);
                           }}>
                           <FaEdit style={{ fontSize: 22 }} />
                        </Button>
                        <Button
                           color={"link"}
                           className={"pt-0 px-0"}
                           aria-label={"delete"}
                           onClick={(_) => deleteTest(test.testId)}>
                           <FaX style={{ fontSize: 22 }} />
                        </Button>
                     </TestTableEntry>
                  ))
               ) : (
                  <div className={"d-flex justify-content-center"}>
                     <h5 color={"danger"}>No Tests</h5>
                  </div>
               )}
            </tbody>
         </table>
      </div>
   );
}

function TestTableEntry({ test = {}, editTest, deleteTest, children }) {
   return (
      <tr key={test.testId} className={"align-middle"}>
         <td>{test.testTitle}</td>
         <td>{test.testDescription}</td>
         <td>{test.created}</td>
         <td>{test.lastUpdated}</td>
         <td className='d-flex align-items-center justify-content-end'>
            {children}
         </td>
      </tr>
   );
}
