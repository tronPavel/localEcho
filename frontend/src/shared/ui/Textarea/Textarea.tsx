import {type TextareaHTMLAttributes, forwardRef, useId} from 'react';
import styles from './Textarea.module.css';
import { classNames } from '@/shared/lib/utils/classNames';

interface TextareaProps extends TextareaHTMLAttributes<HTMLTextAreaElement> {
    error?: string;
    label?: string;
}

export const Textarea = forwardRef<HTMLTextAreaElement, TextareaProps>(({ className, label, error, ...props }, ref) => {
    const id = useId();
    return (
        <div className={styles.wrapper}>
            {label && <label htmlFor={id} className={styles.label}>{label}</label>}
            <textarea
                id={id}
                ref={ref}
                className={classNames(styles.textarea, className, error ? styles.inputError : '')}
                {...props}
            />
            {error && <span className={styles.errorText}>{error}</span>}
        </div>
    );
});
Textarea.displayName = 'Textarea';