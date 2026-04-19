import styles from './ModalHeader.module.css';

interface ModalHeaderProps {
    title: string;
    subtitle?: string;
}

export const ModalHeader = ({ title, subtitle }: ModalHeaderProps) => {
    return (
        <div className={styles.header}>
            <div className={styles.titleWrapper}>
                <h2 className={styles.title}>
                    <span className={styles.accent}>{title.slice(0, 3)}</span>
                    {title.slice(3)}
                </h2>
            </div>
            {subtitle && <p className={styles.subtitle}>{subtitle}</p>}
        </div>
    );
};