import {ReactNode} from 'react';
import Column from './Column.tsx';
import Row from './Row.tsx';
import Button from './Button.tsx';
import './Dialog.css';

export default function Dialog({children, className, title, onClose}: {
    title?: string;
    children?: ReactNode;
    className?: string;
    onClose?: () => void;
}) {
    return <Column className={'Dialog overflow-scroll ' + (className ?? '')}>
        <Row>
            <h3 className='flex-grow'>{title}</h3>
            {onClose && <Button text='x' onClick={onClose} />}
        </Row>
        {children}
    </Column>
}
