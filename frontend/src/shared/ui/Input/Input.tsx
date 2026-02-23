import { type InputHTMLAttributes, forwardRef } from 'react';
import styles from './Input.module.css';
import { classNames } from '@/shared/lib/utils/classNames';

interface InputProps extends InputHTMLAttributes<HTMLInputElement> {
    error?: string;
}

export const Input = forwardRef<HTMLInputElement, InputProps>(({ className, error, ...props }, ref) => {
    return (
        <div className={styles.wrapper}>
            <input ref={ref} className={classNames(styles.input, className)} {...props} />
            {error && <span className={styles.error}>{error}</span>}
        </div>
    );
});
Input.displayName = 'Input';