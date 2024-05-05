import {ReactNode} from 'react';
import Column from './Column.tsx';
import './Dialog.css';

export default function Dialog({children, className}: {
    children: ReactNode;
    className?: string;
}) {
    return <Column className={'Dialog ' + className ?? ''}>
        {children}
    </Column>
}
