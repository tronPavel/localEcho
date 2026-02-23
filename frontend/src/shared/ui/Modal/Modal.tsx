import { createPortal } from "react-dom";
import styles from './Modal.module.css';

interface ModalProps {
    isOpen: boolean;
    onClose: () => void;
    children: React.ReactNode;
}

export const Modal = ({ isOpen, onClose, children }: ModalProps) => {
    if (!isOpen) return null;

    return createPortal(
        <div className={styles.modalOverlay} onClick={onClose}>
            <div className={styles.modal} onClick={e => e.stopPropagation()}>
                <button
                    className={styles.closeButton}
                    onClick={onClose}
                    aria-label="Закрыть"
                >
                    ×
                </button>
                <div className={styles.modalContent}>
                    {children}
                </div>
            </div>
        </div>,
        document.body
    );
};