import {ReactNode} from "react";
import './DataTable.css';

export type DataTableColumnDefinition<T> = {
    field: string,
    label?: string,
    visualize?: (value: T) => ReactNode
};

function TableHead({columns}: { columns: DataTableColumnDefinition<any>[] }) {
    return <thead className='DataTableHead'>
    <tr>
        {
            columns.map(column =>
                <th key={column.field}>
                    {column.label ?? column.field}
                </th>)
        }
    </tr>
    </thead>;
}

function DataTableRow({rowData, columns}: {
    rowData: any,
    columns: DataTableColumnDefinition<any>[]
}) {
    return <tr>
        {
            columns.map(column => {
                return <td key={column.field}>
                    {
                        column.visualize
                            ? column.visualize(rowData)
                            : rowData[column.field]
                    }
                </td>;
            })
        }
    </tr>;
}

export default function DataTable<T>({data, columns}: {
    data: T[],
    columns: DataTableColumnDefinition<any>[]
}) {
    return <table className='DataTable'>
        <TableHead columns={columns}/>
        <tbody>
        {data.map((element, index) => <DataTableRow key={index} rowData={element} columns={columns}/>)}
        </tbody>
    </table>
}
