import authService from "./api-authorization/AuthorizeService";
import React, { useEffect, useState } from "react";
import {
   ItemAdministration,
   ItemAdministrationBody,
   ItemAdministrationBodyEntry,
   ItemAdministrationHeader,
   ItemAdministrationHeaderEntry,
   ItemAdministrationRow,
   ItemAdministrationRowActions,
   ItemAdministrationTable,
   ItemAdministrationTableBody,
   ItemAdministrationTableHeader,
} from "./ItemAdministration";
import { Button } from "reactstrap";
import { Link } from "react-router-dom";

export function UserPortal() {
   const [tests, setTests] = useState([]);

   useEffect(() => {
      GetAvailableTests();
   }, []);

   return (
      <ItemAdministration>
         <ItemAdministrationHeader>Available tests</ItemAdministrationHeader>
         <ItemAdministrationBody>
            <ItemAdministrationTable fallbackColumnSpan={3}>
               <ItemAdministrationTableHeader>
                  <ItemAdministrationRow>
                     <ItemAdministrationHeaderEntry>
                        #
                     </ItemAdministrationHeaderEntry>
                     <ItemAdministrationHeaderEntry>
                        Title
                     </ItemAdministrationHeaderEntry>
                     <ItemAdministrationHeaderEntry>
                        Description
                     </ItemAdministrationHeaderEntry>
                     <ItemAdministrationHeaderEntry
                        className={
                           "d-flex align-items-end justify-content-end pe-4"
                        }>
                        Actions
                     </ItemAdministrationHeaderEntry>
                  </ItemAdministrationRow>
               </ItemAdministrationTableHeader>
               <ItemAdministrationTableBody>
                  {tests.map((test, index) => (
                     <ItemAdministrationRow key={index}>
                        <ItemAdministrationBodyEntry>
                           {index + 1}
                        </ItemAdministrationBodyEntry>
                        <ItemAdministrationBodyEntry>
                           {test.testTitle}
                        </ItemAdministrationBodyEntry>
                        <ItemAdministrationBodyEntry>
                           {test.testDescription}
                        </ItemAdministrationBodyEntry>
                        <ItemAdministrationRowActions>
                           <Link color={"primary"} to={`/Test/${test.testId}`}>
                              <Button className={"btn btn-primary"}>
                                 Take Test
                              </Button>
                           </Link>
                        </ItemAdministrationRowActions>
                     </ItemAdministrationRow>
                  ))}
               </ItemAdministrationTableBody>
            </ItemAdministrationTable>
         </ItemAdministrationBody>
      </ItemAdministration>
   );

   async function GetAvailableTests() {
      const token = await authService.getAccessToken();
      await fetch("/api/Share", {
         headers: !token
            ? {}
            : {
                 Authorization: `Bearer ${token}`,
                 "Content-Type": "application/json",
              },
      })
         .then((res) => {
            if (res.ok) return res.json();
         })
         .then((data) => data && setTests(data))
         .catch((e) => console.log(e));
   }
}
