import styles from './Button.module.css';
import type {ButtonHTMLAttributes, ReactNode} from "react";
import {classNames} from "../../lib/utils/classNames.ts";

interface ButtonProps extends ButtonHTMLAttributes<HTMLButtonElement> {
    variant?: 'primary' | 'secondary' | 'outline';
    size?: 'small' | 'medium' | 'large';
    children: ReactNode;
}

export const Button = ({
                           variant = 'primary',
                           size = 'medium',
                           children,
                           className,
                           ...props
                       }: ButtonProps) => {
    return (
        <button
            className={classNames(styles.button, styles[variant], styles[size], className)}
            {...props}
        >
            {children}
        </button>
    );
};