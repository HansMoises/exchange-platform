import type { HTMLAttributes } from 'react';

type CardProps = HTMLAttributes<HTMLDivElement>;

export default function Card({ className = '', ...props }: CardProps) {
  return <div className={`rounded-lg bg-bg p-md shadow-card transition-shadow duration-200 ${className}`} {...props} />;
}
