import os
from optimum.onnxruntime import ORTModelForSequenceClassification
from transformers import AutoTokenizer
from optimum.onnxruntime.configuration import AutoQuantizationConfig
from optimum.onnxruntime import ORTQuantizer

# Model Choice: "original" DistilBERT is ~260MB.
# We need < 100MB.
# "martin-ha/toxic-comment-model" or "unitary/toxic-bert"
# Let's use a small distilled model if possible, or quantize a standard one.
MODEL_ID = "martin-ha/toxic-comment-model" 
OUTPUT_DIR = "../ProfanityAddon/Resources"

def export_and_quantize():
    print(f"Downloading and exporting {MODEL_ID} to ONNX...")
    
    # 1. Export to ONNX
    # ORTModel handles the export automatically when export=True
    model = ORTModelForSequenceClassification.from_pretrained(MODEL_ID, export=True)
    tokenizer = AutoTokenizer.from_pretrained(MODEL_ID)
    
    # Save the full precision ONNX model first (tmp)
    tmp_dir = "tmp_onnx"
    model.save_pretrained(tmp_dir)
    tokenizer.save_pretrained(tmp_dir)
    
    # 2. Quantize (Dynamic Quantization for INT8)
    print("Quantizing model to INT8...")
    quantizer = ORTQuantizer.from_pretrained(tmp_dir)
    qconfig = AutoQuantizationConfig.avx512(is_static=False, per_channel=False)
    
    # If AVX512 config fails on some generic machines, use arm64 or basic
    # But AVX2/512 is standard for server. Let's try basic dynamic quantization.
    # qconfig = AutoQuantizationConfig.arm64(is_static=False, per_channel=False) 
    
    # Actually, let's use the simplest dynamic quantization config provided by Optimum
    # or handle via standard ORT tools if Optimum acts up. 
    # But usually this works:
    
    export_path = os.path.join(OUTPUT_DIR, "profanity_model.onnx")
    os.makedirs(OUTPUT_DIR, exist_ok=True)
    
    quantizer.quantize(
        save_dir=OUTPUT_DIR,
        quantization_config=qconfig,
    )
    
    # Also save tokenizer resources (vocab.txt) to the output dir
    tokenizer.save_pretrained(OUTPUT_DIR)
    
    # Clean up huge ONNX file
    # (The quantized one is saved in OUTPUT_DIR/model_quantized.onnx usually, let's rename it)
    
    print(f"✅ Model exported to {OUTPUT_DIR}")
    
    # Check size
    for f in os.listdir(OUTPUT_DIR):
        fp = os.path.join(OUTPUT_DIR, f)
        if os.path.isfile(fp):
            size_mb = os.path.getsize(fp) / (1024 * 1024)
            print(f"  - {f}: {size_mb:.2f} MB")

if __name__ == "__main__":
    try:
        import optimum
    except ImportError:
        print("Installing dependencies...")
        os.system("pip install optimum[onnxruntime] transformers")
        
    export_and_quantize()
