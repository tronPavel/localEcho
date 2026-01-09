import {createPortal} from "react-dom";
import './Modal.css';

interface ModalProps {
    isOpen: boolean;
    onClose: () => void;
    children: React.ReactNode;
}

export const Modal = ({isOpen, onClose, children}:ModalProps)=>{
    if(!isOpen) return null;
    return createPortal(
        <div className="modal-overlay" onClick={onClose}>
            <div className="modal" onClick={(e)=>e.stopPropagation()}>
                <button className="modal-close-btn" onClick={onClose}>×</button>
                <div className="modal-content">
                    {children}
                </div>
            </div>
        </div>,
        document.body
    )
}