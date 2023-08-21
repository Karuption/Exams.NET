import {Spinner, Table} from "reactstrap";
import React, {Suspense} from "react";

export function ItemAdministrationHeader({children, ...restProps}) {return <h1 {...restProps}>{children}</h1>;}
export function ItemAdministrationSubtitle({children, ...restProps}) { return <p {...restProps}>{children}</p>;}

export function ItemAdministrationBody({children}) {
    return children;
}

export function ItemAdministration({children}) {
    const childElements = React.Children.toArray(children);
    const itemAdministrationHeader = childElements.find(child=>child.type.name === "ItemAdministrationHeader");
    const itemAdministrationSubtitle = childElements.find(child=>child.type.name === "ItemAdministrationSubtitle");
    const itemAdministrationBody = childElements.find(child=>child.type.name === "ItemAdministrationBody");

    return (
        <>
            {itemAdministrationHeader}
            {itemAdministrationSubtitle}
            {itemAdministrationBody}
        </>
    );
}


export function ItemAdministrationTable({children, fallback = null, fallbackColumnSpan = "2", loading=false, ...restProps}) {
    const childElements = React.Children.toArray(children);
    const tableHeaders = childElements.find(child=>child.type.name === 'ItemAdministrationTableHeader');
    const tableBody = childElements.find(child=>child.type.name === 'ItemAdministrationTableBody');

    const Fallback = () => fallback===null?
        <ItemAdministrationTableBody>
            <ItemAdministrationRow>
                <ItemAdministrationBodyEntry colSpan={fallbackColumnSpan}>
                    <div className={'d-flex justify-content-center'}>
                        <Spinner />
                    </div>
                </ItemAdministrationBodyEntry>
            </ItemAdministrationRow>
        </ItemAdministrationTableBody>
        :fallback

    return (
        <Table {...restProps}>
            {tableHeaders}
            <Suspense fallback={Fallback}>
                {loading!==true?tableBody:<Fallback />}
            </Suspense>
        </Table>
    );
}
export function ItemAdministrationTableHeader({children, ...restProps}) {
    return (
        <thead {...restProps}>
        {children}
        </thead>
    );
}

export function ItemAdministrationTableBody({children, ...restProps}) {
    return (
        <tbody {...restProps}>
        {children}
        </tbody>
    );
}

export function ItemAdministrationRow({children, ...restProps}) {
    const childElement = React.Children.toArray(children);
    const itemFragment = childElement.filter(child => child.type.name === "ItemAdministrationBodyEntry" || child.type.name === "ItemAdministrationHeaderEntry")
    const itemActions = childElement.filter(child => child.type.name === "ItemAdministrationRowActions");
    return (
        <tr {...restProps}>
            {
                itemFragment.map(item => (
                    item
                ))
            }
            {
                itemActions.map(action => action)
            }
        </tr>
    );
}

export function ItemAdministrationRowActions({children, ...restProps}) {
    const actions = React.Children.toArray(children);
    return (
        <td className={'d-flex align-items-end justify-content-end gap-2'}>
            { children }
        </td>
    );
}

export function ItemAdministrationHeaderEntry({children, ...restProps}) {
    return (
        <th {...restProps}>{children}</th>
    );
}

export function ItemAdministrationBodyEntry({children, ...restProps}) {
    return (
        <td {...restProps}>{children}</td>
    )
}