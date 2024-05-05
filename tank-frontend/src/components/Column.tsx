import {MouseEventHandler, ReactNode} from 'react';

import './Column.css'

export default function Column({children, className, onClick}: {
    children: ReactNode;
    className?: string;
    onClick?: MouseEventHandler<HTMLDivElement>;
}) {
    return <div className={'Column flex-column ' + (className ?? '')} onClick={onClick}>
        {children}
    </div>
}

