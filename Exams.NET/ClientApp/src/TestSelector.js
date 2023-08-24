import {Suspense, useEffect, useState} from "react";
import authService from "./components/api-authorization/AuthorizeService";
import {Card, CardBody, CardHeader, CardSubtitle, Spinner} from "reactstrap";

export function TestSelector({callback, viewOnly=false,testId=0}) {
    const [tests, setTests] = useState([]);
    const [loading, setLoading] = useState(true);

    useEffect(() => {
        if(viewOnly)
            GetTest(testId);
        else 
            GetAllTests();
    }, []);
    
    
    return (
        <Suspense fallback={<Spinner />}>
                {
                    tests.map(test => (
                        <Card key={test?.testId} className={'mb-2'}
                              onClick={_ => callback(test)}>
                            <CardHeader>{test?.testTitle}</CardHeader>
                            <CardBody>{test?.testDescription}</CardBody>
                        </Card>
                    ))
                }
        </Suspense>
    );

    async function GetAllTests() {
        const token = await authService.getAccessToken();
        await fetch(`api/admin/Test`, {
            headers: !token ? {} : {'Authorization': `Bearer ${token}`, 'Accept': 'application/json'},
        }).then(res => {
            if (!res.ok)
                console.log(res);
            else
                return res.json();
        })
            .then(data => {setTests(data);setLoading(false);});
    }
    
    async function GetTest(testId) {
        const token = await authService.getAccessToken();
        await fetch(`api/admin/Test/${testId}`, {
            headers: !token ? {} : {'Authorization': `Bearer ${token}`, 'Accept': 'application/json'},
        }).then(res => {
            if (!res.ok)
                console.log(res);
            else
                return res.json();
        })
            .then(data => {setTests([data]);setLoading(false);});
    }
}

