import { type SelectHTMLAttributes, forwardRef } from 'react';
import styles from './Select.module.css';
import { classNames } from '@/shared/lib/utils/classNames';

interface SelectProps extends SelectHTMLAttributes<HTMLSelectElement> {}

export const Select = forwardRef<HTMLSelectElement, SelectProps>(({ className, ...props }, ref) => {
    return <select ref={ref} className={classNames(styles.select, className)} {...props} />;
});
Select.displayName = 'Select';