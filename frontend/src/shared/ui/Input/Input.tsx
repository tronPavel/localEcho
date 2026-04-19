import { type InputHTMLAttributes, forwardRef, useId } from 'react';
import styles from './Input.module.css';
import { classNames } from '@/shared/lib/utils/classNames';

interface InputProps extends InputHTMLAttributes<HTMLInputElement> {
    label?: string;
    error?: string;
}

export const Input = forwardRef<HTMLInputElement, InputProps>(({ className, label, error, ...props }, ref) => {
    const id = useId();
    return (
        <div className={styles.wrapper}>
            {label && <label htmlFor={id} className={styles.label}>{label}</label>}
            <input
                id={id}
                ref={ref}
                className={classNames(styles.input, error ? styles.error : '', className)}
                {...props}
            />
            {error && <span className={styles.errorMessage}>{error}</span>}
        </div>
    );
});