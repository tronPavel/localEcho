import { type TextareaHTMLAttributes, forwardRef } from 'react';
import styles from './Textarea.module.css';
import { classNames } from '@/shared/lib/utils/classNames';

interface TextareaProps extends TextareaHTMLAttributes<HTMLTextAreaElement> {}

export const Textarea = forwardRef<HTMLTextAreaElement, TextareaProps>(({ className, ...props }, ref) => {
    return <textarea ref={ref} className={classNames(styles.textarea, className)} {...props} />;
});
Textarea.displayName = 'Textarea';