import {ReactNode} from "react";

import './Column.css'

export default function Column({children, className}: {
    children: ReactNode,
    className?: string
}) {
    return <div className={'Column flex-column ' + (className ?? '')}>
        {children}
    </div>
}

