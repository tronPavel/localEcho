import type { ReactNode } from 'react';
import { useNavigate } from 'react-router-dom';
import { Modal } from './Modal';

interface RoutedModalProps {
    children: ReactNode;
    title?: string;
    maxWidth?: number;
}

export const RoutedModal = ({ children, title, maxWidth }: RoutedModalProps) => {
    const navigate = useNavigate();

    return (
        <Modal isOpen={true} onClose={() => navigate('/')}>
            {title && <h2>{title}</h2>}
            <div style={{ maxWidth: maxWidth ? `${maxWidth}px` : 'auto' }}>
                {children}
            </div>
        </Modal>
    );
};