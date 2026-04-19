import styles from './VerifiedBadge.module.css';

export const VerifiedBadge = () => (
    <span className={styles.badge} title="Подтвержденный аккаунт">
        <svg viewBox="0 0 24 24" fill="currentColor" width="16" height="16">
            <path d="M9 16.17L4.83 12l-1.42 1.41L9 19 21 7l-1.41-1.41z" />
        </svg>
    </span>
);