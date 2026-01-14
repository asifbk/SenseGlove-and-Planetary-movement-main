import os
import pandas as pd
import matplotlib.pyplot as plt
import numpy as np

# ---------------------------------------------
# LOAD CSV EXACTLY AS RECORDED (NO FILTERING)
# ---------------------------------------------
def load_raw(csv_path):
    print(f"\nLoading: {csv_path}")

    # Load CSV
    df = pd.read_csv(csv_path, encoding='utf-8-sig', comment='#')

    # Convert numeric columns
    df = df.apply(pd.to_numeric, errors='coerce')

    # Remove invalid rows (NaN)
    df = df.dropna()

    return df


# ---------------------------------------------
# PLOT + SAVE PNG (RAW DATA, 3 SUBPLOTS)
# ---------------------------------------------
def plot_and_save_raw(df, title, out_name):
    t = df["time"].values
    V = df["V"].values
    A = df["A"].values
    O = df["O"].values

    plt.figure(figsize=(12, 8))
    plt.suptitle(title, fontsize=16)

    # ---------- O(t) ----------
    ax1 = plt.subplot(3, 1, 1)
    ax1.plot(t, O, color='purple', linewidth=1.2)
    ax1.set_ylabel("O(t)")
    ax1.grid(True, alpha=0.3)

    # ---------- A(t) ----------
    ax2 = plt.subplot(3, 1, 2)
    ax2.plot(t, A, color='orange', linewidth=1.0)
    ax2.set_ylabel("A(t)")
    ax2.grid(True, alpha=0.3)

    # ---------- V(t) ----------
    ax3 = plt.subplot(3, 1, 3)
    ax3.plot(t, V, color='red', linewidth=1.0)
    ax3.set_ylabel("V(t)")
    ax3.set_xlabel("time (s)")
    ax3.grid(True, alpha=0.3)

    plt.tight_layout(rect=[0, 0, 1, 0.95])

    # Save PNG
    output_path = os.path.join(SCRIPT_DIR, out_name + ".png")
    plt.savefig(output_path, dpi=300)
    print(f"💾 Saved: {output_path}")

    plt.show()


# ---------------------------------------------
# AUTO-DISCOVER CSV FILES
# ---------------------------------------------
SCRIPT_DIR = os.path.dirname(os.path.abspath(__file__))
csv_files = [f for f in os.listdir(SCRIPT_DIR) if f.lower().endswith(".csv")]

if not csv_files:
    print("❌ No CSV files found!")
    exit()

print("Detected CSVs:", csv_files)


# ---------------------------------------------
# PROCESS EVERY CSV
# ---------------------------------------------
for csv in csv_files:
    full_path = os.path.join(SCRIPT_DIR, csv)
    df_raw = load_raw(full_path)

    name = os.path.splitext(csv)[0]
    plot_and_save_raw(df_raw, title=name, out_name=name)
