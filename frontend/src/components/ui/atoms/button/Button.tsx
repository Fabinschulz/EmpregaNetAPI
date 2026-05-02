"use client";

import * as React from "react";
import { Slot } from "@radix-ui/react-slot";
import { cva, type VariantProps } from "class-variance-authority";
import type { LucideIcon } from "lucide-react";
import { cn } from "@/utils/lib";
import styles from "./Button.module.scss";

const buttonVariants = cva(styles.root, {
  variants: {
    variant: {
      default: styles.variantDefault,
      primary: styles.variantPrimary,
      destructive: styles.variantDestructive,
      outline: styles.variantOutline,
      secondary: styles.variantSecondary,
      ghost: styles.variantGhost,
      link: styles.variantLink,
    },
    size: {
      default: styles.sizeDefault,
      sm: styles.sizeSm,
      lg: styles.sizeLg,
      icon: styles.sizeIcon,
    },
  },
  defaultVariants: {
    variant: "default",
    size: "default",
  },
});

export type ButtonProps = Omit<React.ButtonHTMLAttributes<HTMLButtonElement>, "className"> &
  VariantProps<typeof buttonVariants> & {
    className?: string;
    asChild?: boolean;
    startIcon?: LucideIcon;
    endIcon?: LucideIcon;
    iconStyleOverrides?: string;
  };

const Button = React.forwardRef<HTMLButtonElement, ButtonProps>(
  (
    {
      className,
      variant,
      size,
      asChild = false,
      startIcon: StartIcon,
      endIcon: EndIcon,
      iconStyleOverrides,
      children,
      ...props
    },
    ref
  ) => {
    const Comp = asChild ? Slot : "button";
    const showIcons = !asChild && (!!StartIcon || !!EndIcon);
    return (
      <Comp className={cn(buttonVariants({ variant, size }), className)} ref={ref} {...props}>
        {asChild ? (
          children
        ) : (
          <>
            {showIcons && StartIcon ? (
              <StartIcon className={cn(styles.icon, styles.iconStart, iconStyleOverrides)} aria-hidden />
            ) : null}
            {children}
            {showIcons && EndIcon ? (
              <EndIcon className={cn(styles.icon, styles.iconEnd, iconStyleOverrides)} aria-hidden />
            ) : null}
          </>
        )}
      </Comp>
    );
  }
);
Button.displayName = "Button";

export { Button, buttonVariants };
