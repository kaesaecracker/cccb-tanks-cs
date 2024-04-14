import {ReactNode} from "react";

import './Row.css';

export default function Row({children, className}: { children: ReactNode, className?: string }) {
    return <div className={'Row flex-row ' + (className ?? '')}>
        {children}
    </div>
}
