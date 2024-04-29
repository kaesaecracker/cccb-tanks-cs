import { ReactNode, useState } from "react";
import './DataTable.css';

export type DataTableColumnDefinition<T> = {
    field: string,
    label?: string,
    visualize?: (value: T) => ReactNode,
    sorter?: (a: T, b: T) => number
};


function DataTableRow({ rowData, columns }: {
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

export default function DataTable<T>({ data, columns, className }: {
    data: T[],
    columns: DataTableColumnDefinition<any>[],
    className?: string
}) {
    const [sortBy, setSortBy] = useState<DataTableColumnDefinition<any> | null>(null);
    const [sortReversed, setSortReversed] = useState(false);

    const headerClick = (column: DataTableColumnDefinition<any>) => {
        console.log('column clicked', column);

        if (column.field === sortBy?.field)
            setSortReversed(prevState => !prevState);
        else if (column.sorter)
            setSortBy(column);
        else
            console.log('cannot sort by', column)
    };

    const dataToDisplay = [...data];
    const actualSorter = sortReversed && sortBy?.sorter
        ? (a: T, b: T) => -sortBy.sorter!(a, b)
        : sortBy?.sorter;

    dataToDisplay.sort(actualSorter)

    return <div className={'DataTable ' + (className ?? '')}>
        <table>
            <thead className='DataTableHead'>
                <tr>
                    {
                        columns.map(column =>
                            <th key={column.field} onClick={() => headerClick(column)}>
                                {column.label ?? column.field}
                            </th>)
                    }
                </tr>
            </thead>
            <tbody>
                {
                    dataToDisplay.map((element, index) =>
                        <DataTableRow key={`${sortBy?.field}-${index}`} rowData={element} columns={columns} />)
                }
            </tbody>
        </table>
    </div>;
}
