import './Button.css';
import {MouseEventHandler} from "react";

export default function Button({text, onClick, className}: {
    text: string,
    onClick?: MouseEventHandler<HTMLButtonElement>,
    className?: string
}) {
    return <button
        className={'Button ' + (className ?? '')}
        onClick={onClick}
    >
        {text}
    </button>
}
