import { type SelectHTMLAttributes, forwardRef, useId } from 'react';
import styles from './Select.module.css';
import { classNames } from '@/shared/lib/utils/classNames';

interface SelectProps extends SelectHTMLAttributes<HTMLSelectElement> {
    label?: string;
    error?: string;
}

export const Select = forwardRef<HTMLSelectElement, SelectProps>(({ className, label, error, children, ...props }, ref) => {
    const id = useId();
    return (
        <div className={styles.wrapper}>
            {label && <label htmlFor={id} className={styles.label}>{label}</label>}
            <div className={styles.selectWrapper}>
                <select id={id} ref={ref} className={classNames(styles.select, error ? styles.error : '', className)} {...props}>
                    {children}
                </select>
                <div className={styles.arrow} />
            </div>
            {error && <span className={styles.errorMessage}>{error}</span>}
        </div>
    );
});