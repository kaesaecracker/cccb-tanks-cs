import {ReactNode} from "react";

import './Row.css';

export default function Row({children, className}: { children: ReactNode, className?: string }) {
    return <div className={'Row ' + (className ?? '')}>
        {children}
    </div>
}
