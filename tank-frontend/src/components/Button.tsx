import './Button.css';
import {MouseEventHandler} from "react";

export default function Button({text, onClick, className, disabled}: {
    text: string,
    onClick?: MouseEventHandler<HTMLButtonElement>,
    className?: string,
    disabled?: boolean
}) {
    return <button
        className={'Button ' + (className ?? '')}
        onClick={onClick}
        disabled={disabled ?? false}
    >
        {text}
    </button>
}
