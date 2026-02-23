import styles from './Icon.module.css';
import type {CSSProperties} from "react";

interface IconProps {
    type: 'up' | 'down' | 'other';
    style?: CSSProperties;
}

export const Icon = ({ type, style }: IconProps) => {
    const icons = {
        up: '↑',
        down: '↓',
        other: '',
    };
    return <span className={styles.icon} style={style}>{icons[type]}</span>;
};